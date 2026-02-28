# 🏢 ARP - Sistema de Análise de Riscos Psicossociais

Aplicação Graphql responsável por proporcionar gestão de análise de riscos psicossociais para empresas e colocaboradoes.

## 💻 RODAR PROJETO 

Temos duas opções

1 - DOCKETA primeira é rodar diretamente pelo IIS 

### - DOCKER

``` 
docker compose -f docker-compose.yml up -d
  ```

### - IIS

Através do Visual Studio



## 🛠️ FERRAMENTAS E TECNOLOGIAS 

Este sistema foi construido na última versão disponível do .Net Core, ou seja, versão 10.

Algumas da bibliotecas utilizadas:

- GitMoji
- Entiti Framework Core com Postgres
- Identity (Gerenciamento de Usuários e Token)

Banco: Neon
Servidor: Render

## ❕ESTRUTURA DO PROJETO

- ARP : Parte Prinicpal do Projeto - Onde existem Querys e Mutations;
- ARP.Entity : Entidades / Modelos dos Objetos com relação com as tabelas do Bando de Dados;
- ARP.Infra : Contexto, Migrations e Funcionalidades suportam o projeto;
- ARP.Service : Não é obrigatório seu uso, mas é o ideal para concentrar a lógica de processamento como regras de negócio;

## ❗IMPORTANTE SABER

### Como Rodar Migrations

<p> Ao subir a aplicação ele cria a base de dados automaticamente, considerando o arquivo create tables.sql dentro da pasta SqlScripts na raiz do projeto.</p> 

//dotnet ef migrations add InitialCreate --project ARP.Infra --startup-project ARP
//dotnet ef database update --project ARP.Infra --startup-project ARP

### Sobre Configurações

<p> Existe um arquivo chamado launchSettings.json e appSettings.json.
Esté é responsável por aramazenar dados sensiveis ao projeto
como Conexão de Banco, Chave JWT, Tempo de expiração do token, Credenciais entre outras
Para o docket utilizase ...</p>

## ☁️ ACESSOS

### RENDER - Aplicação
### NEON - Banco de Dados

``` 
Endereço de e-mail do usuário root : leomar_sartor@unochapeco.edu.br
Senha: Xilindr0
```

## 🔑 TOKEN

Padrão JWT

![Padrão JWT](https://github.com/leomar-sartor/Mentant/blob/main/documentation/ModeloJWT.png)

## 🔎 LOGS

<p> Existe uma tabela (LOG) responsável por armazenar todos os registros (INSERT, UPDATE, DELETE) ocorrido dentro do sistema, considerando
ID do uisuário, instrução sql executada e seus paramêtros, também um campo mensagem caso ocorra uma excessão, para agilizar a identificação do problema.</p>

## 🔗 QUERYS E MUTATIONS (Insomnia)

A documentação já está disponivel no projeto, vide: 

![Swagger](https://github.com/leomar-sartor/Mentant/blob/main/documentation/DocumentacaoSwagger.png)

Ou você pode as utilizar as request do insomnia exportadas [Aqui](https://github.com/leomar-sartor/Mentant/blob/main/documentation/Insomnia_Request.json). É só importar.


## 💯 Arquitetura

![Arquitetura](https://github.com/leomar-sartor/Mentant/blob/main/documentation/JBS.png)


## :shipit: Não gostou da documentação! Documenta o negócio aí e melhora, assim eu apreendo com você!

# PADRÃO DE COMMITS

Ícone - tipo : descrição
🔑 - feat : crud de pessoas

# OBSERVAÇÕES FINAIS

1 - AUTH

* Token com expiração de 15m;
* Já existe RefreshToken;
* Já existe Logout;

2 - CADASTRO EMPRESA

3 - CADASTRO SETOR

4 - EXEMPLO

5 - OUTRAS

* Logs ainda não forma implementados;

Links
//https://chillicream.com/docs/hotchocolate/v13/defining-a-schema/object-types
//https://fiyazhasan.work/tag/graphql/page/2/
//https://github.com/fiyazbinhasan/GraphQLCoreFromScratch


QUESTIONARIO VERI
1 - A;
2 - C;
3 - C;
4 - B;
5 - A;
6 - C;
7 - C;
8 - C;

