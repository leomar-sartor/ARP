using ARP.Infra;
using ARP.Modules.Pesquisa.Types;
using ARP.Modules.Pessoa;
using ARP.Modules.Pessoa.Types;
using Microsoft.EntityFrameworkCore;

namespace ARP.Modules.Pesquisa
{
    [ExtendObjectType("Mutation")]
    public class PesquisaMutation
    {
        private readonly ILogger<PesquisaMutation> _logger;

        public PesquisaMutation(
            ILogger<PesquisaMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Cadastrar um nova pesquisa")]
        public async Task<Entity.Pesquisa> CreatePesquisa(
        PesquisaInput input,
        [Service] Context context)
        {
            var entity = new Entity.Pesquisa
            {
                Nome = input.Nome,
                DataInicial = input.DataInicial,
                DataFinal = input.DataFinal
            };

            //if(input.Questoes. == null || !input.Questoes.Any())
            //{
            //    throw new ArgumentException("A pesquisa deve conter pelo menos uma questão.");
            //}

            foreach (var questaoInput in input.Questoes)
            {
                var questao = new Entity.Questao
                {
                    Titulo = questaoInput.Titulo,
                    Tipo = questaoInput.Tipo,
                    Obrigatoria = questaoInput.Obrigatoria,
                    MultiplasRespostas = questaoInput.MultiplasRespostas,
                    MaximoDeCaracteres = questaoInput.MaximoDeCaracteres
                };


                if (questaoInput.Opcoes != null && questaoInput.Opcoes.Any())
                {
                    foreach (var opcaoInput in questaoInput.Opcoes)
                    {
                        var opcao = new Entity.QuestaoOpcao
                        {
                            Ordem = opcaoInput.Ordem,
                            Descricao = opcaoInput.Descricao
                        };

                        questao.Opcoes.Add(opcao);
                    }
                }

                entity.Questoes.Add(questao);
            }

            context.Pesquisas.Add(entity);

            await context.SaveChangesAsync();

            return entity;
        }

        [GraphQLDescription("Cadastrar uma resposta")]
        public async Task<List<Entity.QuestaoResposta>> CreateResposta(
        RespostaInput input,
        [Service] Context context)
        {
            // Valida se o token é de um convite válido e não concluído
            var convite = await context.Convites
                .Include(c => c.Pesquisa)
                .FirstOrDefaultAsync(c => c.Token == input.Token);

            if (convite is null)
                throw new ArgumentException("Token inválido.");

            if (convite.Status == Entity.Enums.Status.Completo)
                throw new ArgumentException("Esta pesquisa já foi respondida.");

            if (DateTime.UtcNow > convite.Pesquisa.DataFinal)
                throw new ArgumentException("O prazo para responder esta pesquisa encerrou.");

            // Marca como em progresso se ainda pendente
            if (convite.Status == Entity.Enums.Status.Pendente)
            {
                convite.Status = Entity.Enums.Status.EmProgresso;
                convite.IniciadoEm = DateTime.UtcNow;
            }

            var respostas = new List<Entity.QuestaoResposta>();
            var questao = await context.Questoes
                .Include(q => q.Opcoes)
                .FirstOrDefaultAsync(q => q.Id == input.QuestaoId);

            if (questao is not null)
                if (questao.Tipo == Entity.Enums.TipoQuestao.Opcao)
                {
                    var opcoesValidas = questao.Opcoes.Select(o => o.Id).ToHashSet();

                    if (input.QuestaoOpcaoIds == null || !input.QuestaoOpcaoIds.Any())
                    {
                        throw new ArgumentException("A questão permite múltiplas respostas, portanto é necessário informar as opções selecionadas.");
                    }

                    if (questao.MultiplasRespostas)
                    {
                        foreach (var opcaoId in input.QuestaoOpcaoIds)
                        {
                            if (!opcoesValidas.Contains(opcaoId))
                            {
                                throw new ArgumentException($"A opção com ID {opcaoId} não é válida para a questão {questao.Titulo}.");
                            }

                            var resposta = new Entity.QuestaoResposta
                            {
                                Token = input.Token,
                                QuestaoId = input.QuestaoId,
                                QuestaoOpcaoId = opcaoId
                            };

                            respostas.Add(resposta);
                            context.QuestaoRespostas.Add(resposta);
                        }
                    }

                    if (questao.MultiplasRespostas == false)
                    {
                        if (input.QuestaoOpcaoIds.Length > 1)
                        {
                            throw new ArgumentException("A questão permite apenas uma resposta.");
                        }

                        var resposta = new Entity.QuestaoResposta
                        {
                            Token = input.Token,
                            QuestaoId = input.QuestaoId,
                            QuestaoOpcaoId = input.QuestaoOpcaoIds.First()
                        };

                        respostas.Add(resposta);
                        context.QuestaoRespostas.Add(resposta);


                    }

                }
                else if (questao.Tipo == Entity.Enums.TipoQuestao.Texto)
                {
                    var resposta = new Entity.QuestaoResposta
                    {
                        Token = input.Token,
                        QuestaoId = input.QuestaoId,
                        TextoResposta = input.TextoResposta
                    };

                    respostas.Add(resposta);
                    context.QuestaoRespostas.Add(resposta);
                }


            await context.SaveChangesAsync();

            return respostas;
        }

        // Disparar pesquisa para colaboradores
        [GraphQLDescription("Disparar pesquisa para colaboradores")]
        public async Task<bool> DispararPesquisa(
            long pesquisaId,
            long[] colaboradorIds,
            [Service] Context context,
            CancellationToken ct)
        {
            var pesquisa = await context.Pesquisas
                .FirstOrDefaultAsync(p => p.Id == pesquisaId, ct)
                ?? throw new ArgumentException("Pesquisa não encontrada.");

            foreach (var colaboradorId in colaboradorIds)
            {
                // Evita duplicata: um colaborador não recebe 2 convites para a mesma pesquisa
                var jaExiste = await context.Convites
                    .AnyAsync(c => c.ColaboradorId == colaboradorId
                                && c.PesquisaId == pesquisaId, ct);
                if (jaExiste) continue;

                var convite = new Entity.Convite
                {
                    Token = Guid.NewGuid().ToString("N"),
                    ColaboradorId = colaboradorId,
                    PesquisaId = pesquisaId,
                    EnviadoEm = DateTime.UtcNow,
                    Status = Entity.Enums.Status.Pendente
                };
                context.Convites.Add(convite);
            }

            await context.SaveChangesAsync(ct);
            return true;
        }

        // Retomar ou iniciar pesquisa pelo token
        [GraphQLDescription("Buscar progresso da pesquisa pelo token do convite")]
        public async Task<PesquisaSessaoPayload> GetSessaoPesquisa(
            string token,
            [Service] Context context,
            CancellationToken ct)
        {
            var convite = await context.Convites
                .Include(c => c.Pesquisa).ThenInclude(p => p.Questoes).ThenInclude(q => q.Opcoes)
                .FirstOrDefaultAsync(c => c.Token == token, ct)
                ?? throw new ArgumentException("Token inválido.");

            if (convite.Status == Entity.Enums.Status.Completo)
                throw new ArgumentException("Pesquisa já concluída.");

            var rascunho = await context.PesquisaRascunhos
                .FirstOrDefaultAsync(r => r.Token == token, ct);

            return new PesquisaSessaoPayload(
                Pesquisa: convite.Pesquisa,
                UltimaQuestaoRespondidaId: rascunho?.UltimaQuestaoRespondidaId,
                RespostasParciais: rascunho?.RespostasParciais
            );
        }

        // Finalizar pesquisa
        [GraphQLDescription("Finalizar pesquisa e registrar conclusão")]
        public async Task<bool> FinalizarPesquisa(
            string token,
            [Service] Context context,
            CancellationToken ct)
        {
            var convite = await context.Convites
                .FirstOrDefaultAsync(c => c.Token == token, ct)
                ?? throw new ArgumentException("Token inválido.");

            convite.Status = Entity.Enums.Status.Completo;
            convite.ConcluidoEm = DateTime.UtcNow;

            // Remove o rascunho
            var rascunho = await context.PesquisaRascunhos
                .FirstOrDefaultAsync(r => r.Token == token, ct);
            if (rascunho is not null)
                context.PesquisaRascunhos.Remove(rascunho);

            await context.SaveChangesAsync(ct);
            return true;
        }

        [GraphQLDescription("Salvar progresso parcial da pesquisa")]
        public async Task<bool> AutoSavePesquisa(
            string token,
            long ultimaQuestaoRespondidaId,
            string? respostasParciais,
            [Service] Context context,
            CancellationToken ct)
        {
            var convite = await context.Convites
                .FirstOrDefaultAsync(c => c.Token == token, ct)
                ?? throw new ArgumentException("Token inválido.");

            if (convite.Status == Entity.Enums.Status.Completo)
                throw new ArgumentException("Pesquisa já concluída.");

            var rascunho = await context.PesquisaRascunhos
                .FirstOrDefaultAsync(r => r.Token == token, ct);

            if (rascunho is null)
            {
                rascunho = new Entity.PesquisaRascunho
                {
                    Token = token,
                    PesquisaId = convite.PesquisaId,
                    UltimaQuestaoRespondidaId = ultimaQuestaoRespondidaId,
                    RespostasParciais = respostasParciais,
                    UltimaAtualizacao = DateTime.UtcNow
                };
                context.PesquisaRascunhos.Add(rascunho);
            }
            else
            {
                rascunho.UltimaQuestaoRespondidaId = ultimaQuestaoRespondidaId;
                rascunho.RespostasParciais = respostasParciais;
                rascunho.UltimaAtualizacao = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(ct);
            return true;
        }
    }
}
