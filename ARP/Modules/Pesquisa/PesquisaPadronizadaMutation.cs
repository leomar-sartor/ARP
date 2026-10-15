using ARP.Entity.Cadastros;
using ARP.Entity.Enums;
using ARP.Entity.Pesquisas;
using ARP.Infra;
using ARP.Modules.Auth;
using ARP.Modules.Pesquisa.Types;
using ARP.Service;
using ARP.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using PesquisaEntity = ARP.Entity.Pesquisas.Pesquisa;

namespace ARP.Modules.Pesquisa
{
    [ExtendObjectType("Mutation")]
    public sealed class PesquisaPadronizadaMutation
    {
        [GraphQLDescription("Criar pesquisa com escala padronizada de frequência")]
        public async Task<PesquisaEntity> CreatePesquisaPadronizada(
            PesquisaPadronizadaInput input,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.StandardSurveyCreate, cancellationToken);

            var tenantId = activeContext.TenantId
                ?? throw new GraphQLException("Selecione um tenant para criar uma pesquisa padronizada.");
            var companyIds = await ValidateInputAndGetCompanyIdsAsync(
                input, context, activeContext, tenantId, cancellationToken);
            await permissionAuthorizationService.RequireCompaniesAsync(context, activeContext,
                companyIds, PermissionCodes.StandardSurveyCreate, cancellationToken);
            var categoryNames = await GetCategoryNamesAsync(input, context, cancellationToken);

            var survey = new PesquisaEntity
            {
                Nome = input.Nome.Trim(),
                DataInicial = input.DataInicial,
                DataFinal = input.DataFinal,
                TenantId = tenantId,
                Tipo = TipoPesquisa.Padronizada
            };

            AddCompaniesAndQuestions(survey, input, tenantId, companyIds, categoryNames);
            context.Pesquisas.Add(survey);
            await context.SaveChangesAsync(cancellationToken);
            return survey;
        }

        [GraphQLDescription("Editar pesquisa padronizada ainda não publicada")]
        public async Task<PesquisaEntity?> UpdatePesquisaPadronizada(
            long id,
            PesquisaPadronizadaInput input,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.StandardSurveyEdit, cancellationToken);

            var tenantId = activeContext.TenantId
                ?? throw new GraphQLException("Selecione um tenant para editar a pesquisa.");
            var survey = await context.Pesquisas
                .Include(item => item.Questoes)
                    .ThenInclude(question => question.Opcoes)
                .Include(item => item.Questoes)
                    .ThenInclude(question => question.Respostas)
                .Include(item => item.Convites)
                .Include(item => item.EmpresasVinculadas)
                .FirstOrDefaultAsync(item => item.Id == id
                    && item.TenantId == tenantId
                    && item.Tipo == TipoPesquisa.Padronizada,
                    cancellationToken);

            if (survey is null)
                return null;
            await permissionAuthorizationService.RequireCompaniesAsync(context, activeContext,
                survey.EmpresasVinculadas.Select(link => link.EmpresaId),
                PermissionCodes.StandardSurveyEdit, cancellationToken);
            if (survey.PublicadaEm.HasValue || survey.Convites.Count != 0
                || survey.Questoes.Any(question => question.Respostas.Count != 0))
                throw new GraphQLException("Pesquisas publicadas ou distribuídas não podem ser alteradas; duplique para criar uma nova versão.");

            var companyIds = await ValidateInputAndGetCompanyIdsAsync(
                input, context, activeContext, tenantId, cancellationToken);
            var categoryNames = await GetCategoryNamesAsync(input, context, cancellationToken);

            await permissionAuthorizationService.RequireCompaniesAsync(context, activeContext,
                companyIds, PermissionCodes.StandardSurveyEdit, cancellationToken);
            survey.Nome = input.Nome.Trim();
            survey.DataInicial = input.DataInicial;
            survey.DataFinal = input.DataFinal;
            context.Questoes.RemoveRange(survey.Questoes);
            context.PesquisaEmpresas.RemoveRange(survey.EmpresasVinculadas);
            survey.Questoes.Clear();
            survey.EmpresasVinculadas.Clear();
            AddCompaniesAndQuestions(survey, input, tenantId, companyIds, categoryNames);

            await context.SaveChangesAsync(cancellationToken);
            return survey;
        }

        [GraphQLDescription("Publicar pesquisa padronizada imutável")]
        public async Task<bool> PublishPesquisaPadronizada(
            long id,
            [Service] Context context,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.StandardSurveyPublish, cancellationToken);

            var tenantId = activeContext.TenantId
                ?? throw new GraphQLException("Selecione um tenant para publicar a pesquisa.");
            var survey = await context.Pesquisas
                .Include(item => item.Questoes)
                    .ThenInclude(question => question.Opcoes)
                .Include(item => item.EmpresasVinculadas)
                .FirstOrDefaultAsync(item => item.Id == id
                    && item.TenantId == tenantId
                    && item.Tipo == TipoPesquisa.Padronizada,
                    cancellationToken)
                ?? throw new GraphQLException("Pesquisa padronizada não encontrada no tenant ativo.");

            await permissionAuthorizationService.RequireCompaniesAsync(context, activeContext,
                survey.EmpresasVinculadas.Select(link => link.EmpresaId),
                PermissionCodes.StandardSurveyPublish, cancellationToken);
            if (survey.PublicadaEm.HasValue)
                return true;
            if (survey.EmpresasVinculadas.Count == 0 || survey.Questoes.Count == 0)
                throw new GraphQLException("Vincule pelo menos uma empresa e cadastre ao menos uma pergunta antes de publicar.");
            if (survey.Questoes.Any(question => question.CategoriaId is null
                || question.Tipo != TipoQuestao.Opcao
                || question.MultiplasRespostas
                || !PesquisaPadronizadaDefinition.HasFixedOptions(question)))
                throw new GraphQLException("A pesquisa possui perguntas fora do formato padronizado.");

            survey.PublicadaEm = DateTime.UtcNow;
            survey.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        [GraphQLDescription("Disponibilizar pesquisa padronizada aos colaboradores da empresa ativa")]
        public async Task<bool> DispararPesquisaPadronizada(
            long pesquisaId,
            long[] colaboradorIds,
            [Service] Context context,
            [Service] IConfiguration configuration,
            [Service] EmailService emailService,
            [Service] TenantContextResolver tenantContextResolver,
            [Service] PermissionAuthorizationService permissionAuthorizationService,
            CancellationToken cancellationToken)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, cancellationToken);
            await permissionAuthorizationService.RequireAsync(
                context, activeContext, PermissionCodes.StandardSurveyManage, cancellationToken);

            var tenantId = activeContext.TenantId
                ?? throw new GraphQLException("Selecione um tenant para disparar a pesquisa.");
            var companyId = activeContext.CompanyId
                ?? throw new GraphQLException("Selecione uma empresa para disparar a pesquisa.");

            var survey = await context.Pesquisas
                .Include(item => item.EmpresasVinculadas)
                .FirstOrDefaultAsync(item => item.Id == pesquisaId
                    && item.TenantId == tenantId
                    && item.Tipo == TipoPesquisa.Padronizada,
                    cancellationToken)
                ?? throw new GraphQLException("Pesquisa padronizada não encontrada no tenant ativo.");

            if (!survey.PublicadaEm.HasValue)
                throw new GraphQLException("Publique a pesquisa antes de disponibilizá-la.");
            if (!survey.EmpresasVinculadas.Any(link => link.EmpresaId == companyId))
                throw new GraphQLException("A pesquisa não está vinculada à empresa ativa.");

            var distinctCollaboratorIds = colaboradorIds.Distinct().ToArray();
            if (distinctCollaboratorIds.Length == 0)
                throw new GraphQLException("Selecione ao menos um colaborador.");

            var collaborators = await context.Colaboradores
                .Where(collaborator => distinctCollaboratorIds.Contains(collaborator.Id)
                    && collaborator.Ativo
                    && context.ColaboradorEmpresas.Any(link => link.ColaboradorId == collaborator.Id
                        && link.EmpresaId == companyId
                        && link.Empresa.Ativo
                        && link.Ativo))
                .ToListAsync(cancellationToken);
            if (collaborators.Count != distinctCollaboratorIds.Length)
                throw new GraphQLException("Um ou mais colaboradores não pertencem à empresa ativa.");

            var hmacKey = ResolveHmacKey(configuration);
            foreach (var collaborator in collaborators)
            {
                if (await context.Convites.AnyAsync(invitation => invitation.PesquisaId == pesquisaId
                    && invitation.ColaboradorId == collaborator.Id,
                    cancellationToken))
                    continue;

                var invitation = new Convite
                {
                    Token = Guid.NewGuid().ToString("N"),
                    Hash = HashHelper.Generate(collaborator.Cpf, hmacKey),
                    ColaboradorId = collaborator.Id,
                    PesquisaId = survey.Id,
                    EmpresaId = companyId,
                    EnviadoEm = DateTime.UtcNow,
                    Status = Status.Pendente
                };
                context.Convites.Add(invitation);

                await emailService.EnviarEmail(
                    nomeRemetente: "Remetente",
                    emailRemetente: "noreply@brgestao.net",
                    nomeDestinario: collaborator.Nome,
                    emailDestinario: collaborator.Email,
                    mensagem: $"http://localhost:5173/survey?token={invitation.Token}",
                    cancellationToken: cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static async Task<long[]> ValidateInputAndGetCompanyIdsAsync(
            PesquisaPadronizadaInput input,
            Context context,
            ActiveTenantContext activeContext,
            long tenantId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input.Nome))
                throw new GraphQLException("O nome da pesquisa é obrigatório.");
            if (input.DataInicial.HasValue && input.DataFinal.HasValue
                && input.DataInicial.Value > input.DataFinal.Value)
                throw new GraphQLException("A data inicial não pode ser posterior à data final.");
            if (input.Questoes is null || input.Questoes.Count == 0)
                throw new GraphQLException("A pesquisa deve possuir ao menos uma pergunta.");
            if (input.Questoes.Any(question => string.IsNullOrWhiteSpace(question.Titulo) || question.CategoriaId <= 0))
                throw new GraphQLException("Cada pergunta deve possuir título e uma categoria válida.");

            var categoryIds = input.Questoes.Select(question => question.CategoriaId).Distinct().ToArray();
            var activeCategoryIds = await context.Categorias
                .Where(category => category.Ativo && categoryIds.Contains(category.Id))
                .Select(category => category.Id)
                .ToArrayAsync(cancellationToken);
            if (activeCategoryIds.Length != categoryIds.Length)
                throw new GraphQLException("Uma ou mais categorias informadas não existem ou estão inativas.");

            var companyIds = input.EmpresaIds.Distinct().ToArray();
            if (companyIds.Length == 0 || companyIds.Any(id => id <= 0))
                throw new GraphQLException("Associe a pesquisa a pelo menos uma empresa válida.");

            var companies = await context.Empresas
                .Where(company => company.Ativo
                    && company.TenantId == tenantId
                    && companyIds.Contains(company.Id))
                .Select(company => company.Id)
                .ToArrayAsync(cancellationToken);
            if (companies.Length != companyIds.Length)
                throw new GraphQLException("Uma ou mais empresas não pertencem ao tenant ativo.");

            if (!activeContext.IsAdmin)
            {
                var authorizedCompanyIds = await context.UsuariosEmpresas
                    .Where(membership => membership.UsuarioId == activeContext.UserId
                        && membership.Ativo
                        && companyIds.Contains(membership.EmpresaId))
                    .Select(membership => membership.EmpresaId)
                    .ToArrayAsync(cancellationToken);
                if (companyIds.Except(authorizedCompanyIds).Any())
                    throw new GraphQLException("A pesquisa não pode ser vinculada a empresas sem autorização.");
            }

            return companyIds;
        }

        private static void AddCompaniesAndQuestions(
            PesquisaEntity survey,
            PesquisaPadronizadaInput input,
            long tenantId,
            IEnumerable<long> companyIds,
            IReadOnlyDictionary<long, string> categoryNames)
        {
            foreach (var companyId in companyIds)
            {
                survey.EmpresasVinculadas.Add(new PesquisaEmpresa
                {
                    TenantId = tenantId,
                    EmpresaId = companyId
                });
            }

            for (var index = 0; index < input.Questoes.Count; index++)
            {
                survey.Questoes.Add(PesquisaPadronizadaDefinition.CreateQuestion(
                    input.Questoes[index], index + 1, categoryNames[input.Questoes[index].CategoriaId]));
            }
        }

        private static async Task<IReadOnlyDictionary<long, string>> GetCategoryNamesAsync(
            PesquisaPadronizadaInput input,
            Context context,
            CancellationToken cancellationToken)
        {
            var categoryIds = input.Questoes.Select(question => question.CategoriaId).Distinct().ToArray();
            return await context.Categorias
                .Where(category => categoryIds.Contains(category.Id))
                .ToDictionaryAsync(category => category.Id, category => category.Nome, cancellationToken);
        }

        private static string ResolveHmacKey(IConfiguration configuration)
        {
            var key = Environment.GetEnvironmentVariable("KEY_HMAC") ?? configuration["KEY_HMAC"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("KEY_HMAC não está configurada.");
            return key;
        }
    }
}
