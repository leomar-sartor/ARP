using ARP.Infra;
using ARP.Modules.Pesquisa.Types;
using ARP.Modules.Pessoa;
using ARP.Modules.Pessoa.Types;

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
    }
}
