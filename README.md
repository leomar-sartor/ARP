# 🏢 RH APP

Aplicação REST responsável por proporcionar gestão de eventos e controle de qualidade da JBS. 

## 💻 RODAR PROJETO - DOCKER

Temos duas opções aqui. A primeira é rodar diretamente pelo IIS através do Visual Studio, gerando containers auxiliares para LDAP e Postgress, ajustando as configurações.
A segunda é rodar o comando a seguir, onde tudo está em containers.

``` 
docker compose -f docker-compose.yml up -d
  ```

## 🔧 SQL AUXILIAR

Script prontos para conferir dados:

``` 
select * from tipoevento t
delete from tipoevento where Id >= 0

select * from evento e  
delete from evento where Id >= 0

select * from documento d 
delete from documento where Id >= 0

select * from log
delete from log where Id >= 0

select * from unidade u
delete from unidade where Id >= 0

select * from clima c
delete from clima where Id >= 0

select * from setor s
delete from setor where Id >= 0

select * from cluster c
delete from cluster where Id >= 0

select * from analise a
delete from analise where Id >= 0
  ```

## 🛠️ TECNOLOGIAS

Este sistema foi construido na última versão disponível do .Net Core, ou seja, versão 7.
Em conjunto com este projeto, foram utilizados Nuguets (pacotes de bibliotecas) que auxiliam o sistema a fazer o que se propem, sendo algumas delas:

- Npgsql (Conexão com Postgres)
- Dapper (Micro ORM)
- DirectoryServices (Integração com LDAP)
- IdentityModel (Gerenciamento de Token)
- AWSSDK.S3 (Amazon Storage S3)

## ❕ESTRUTURA DO PROJETO

- JBS : Parte Prinicpal do Projeto
- JBS.Entity : Entidades/ Modelos dos Objetos
- JBS.Infra : Objetos que auxiliam na conexão, mapeamento de objetos e configurações gerais
- JBS.Repo : Classe responsáveis por ligar os dados do Banco com as Entidades
- JBS.Service : Nào é obrigatório seu uso, mas seia o ideal para consumir cenários mais complexos de dados (quando envolvem mais que um repositório, ou regras mais complexas)

## ❗IMPORTANTE SABER

### Sobre Banco de Dados

<p> Ao subir a aplicação ele cria a base de dados automaticamente, considerando o arquivo create tables.sql dentro da pasta SqlScripts na raiz do projeto.</p> 

### Sobre Configurações

<p> Existe um arquivo chamado appSettings.json.
Esté é responsável por aramazenar dados sensiveis ao projeto
como Conexão de Banco, Conexão com LDAP, Credenciais S3 Amazon e Chave secreta JWT </p>

## ☁️ ACESSO AMAZON S3 (Storage)

Acesso: [Clique aqui](https://signin.aws.amazon.com/signin?redirect_uri=https%3A%2F%2Fs3.console.aws.amazon.com%2Fs3%2Fbuckets%2Fjbs-api-s3%3FbucketType%3Dgeneral%26prefix%3Dmedias%252F%26region%3Dus-east-1%26state%3DhashArgs%2523%26isauthcode%3Dtrue&client_id=arn%3Aaws%3Aiam%3A%3A015428540659%3Auser%2Fs3&forceMobileApp=0&code_challenge=TgDYXSnMK5b9meWWU-RNGSPB3hi8CeZA5nV5H06nnCE&code_challenge_method=SHA-256) !

``` 
Endereço de e-mail do usuário root : leomar_sartor@unochapeco.edu.br
Senha: Xilindr0
```

![AWS S3](https://github.com/leomar-sartor/Mentant/blob/main/documentation/CredentialS3.png)


## 🔓 LDAP / LOGIN

Por hora está sendo usado um usuário padrão definido em código fonte


``` 
{
  "UserName": "admin",
  "Password": "123456"
}
```

## 🔑 TOKEN

Padrão JWT

![Padrão JWT](https://github.com/leomar-sartor/Mentant/blob/main/documentation/ModeloJWT.png)

## 🔎 LOGS

<p> Existe uma tabela (LOG) responsável por armazenar todos os registros (INSERT, UPDATE, DELETE) ocorrido dentro do sistema, considerando
ID do uisuário, instrução sql executada e seus paramêtros, também um campo mensagem caso ocorra uma excessão, para agilizar a identificação do problema.</p>

## 🔗 Requests (Swagger e Insomnia)

A documentação já está disponivel no projeto, vide: 

![Swagger](https://github.com/leomar-sartor/Mentant/blob/main/documentation/DocumentacaoSwagger.png)

Ou você pode as utilizar as request do insomnia exportadas [Aqui](https://github.com/leomar-sartor/Mentant/blob/main/documentation/Insomnia_Request.json). É só importar.


## 💯 Arquitetura

![Arquitetura](https://github.com/leomar-sartor/Mentant/blob/main/documentation/JBS.png)


## :shipit: Acha que pode fazer melhor! Provoco você a documentar o negócio aí.