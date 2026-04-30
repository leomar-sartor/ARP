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

        [GraphQLDescription("Cadastrar um nova pesquisa")]
        public async Task<List<Entity.QuestaoResposta>> CreateResposta(
        RespostaInput input,
        [Service] Context context)
        {
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
    }
}
