# 🏢 ARP - Sistema de Análise de Riscos Psicossociais

Aplicação Graphql responsável por proporcionar gestão de análise de riscos psicossociais para empresas e colocaboradoes.

## 💻 RODAR PROJETO 

Temos duas opções

1 - DOCKETA primeira é rodar diretamente pelo IIS 

### - DOCKER

``` 
nerdctl build -t arpapp:latest -f .\Dockerfile .
nerdctl run -p 8080:80 --name=arpappcontainer --env-file .env.example -d arpapp:latest
  ```

### - IIS

Através do Visual Studio



## 🛠️ FERRAMENTAS E TECNOLOGIAS 

Este sistema foi construido na última versão disponível do .Net Core, ou seja, versão 10.

Algumas da bibliotecas utilizadas:

- GitMoji
- Entiti Framework Core com Postgres
- Identity (Gerenciamento de Usuários e Token)
- HotChocolate versão 15 - (https://chillicream.com/docs/hotchocolate/v15)

Banco: Neon
Servidor: Render

## ❕ESTRUTURA DO PROJETO

- ARP : Parte Prinicpal do Projeto - Onde existem Querys e Mutations;
- ARP.Entity : Entidades / Modelos dos Objetos com relação com as tabelas do Bando de Dados;
- ARP.Infra : Contexto, Migrations;
- ARP.Utils : Funcionalidades suportam o projeto;
- ARP.Service : Não é obrigatório seu uso, mas é o ideal para concentrar a lógica de processamento como regras de negócio;

## ❗IMPORTANTE SABER

### Como Rodar Migrations

<p> Ao subir a aplicação ele cria a base de dados automaticamente, considerando o arquivo create tables.sql dentro da pasta SqlScripts na raiz do projeto.</p> 

Open DevloperPowerShell
dotnet tool install --global dotnet-ef --version 10.0.3
dotnet ef --version

//dotnet ef migrations add MinhaDescricao --project ARP.Infra --startup-project ARP
//dotnet ef database update --project ARP.Infra --startup-project ARP

### Sobre Configurações

Segredos (`CONNECTION_STRING`, `JWT_KEY`, `KEY_HMAC`, `SMTP_USER`, `SMTP_PASSWORD`) **não** devem ficar em `appsettings.json` nem em `launchSettings.json`.

| Ambiente | Onde configurar |
|----------|-----------------|
| Local | .NET User Secrets (recomendado) ou arquivo `.env` (gitignored) |
| Docker | `--env-file .env` (copie de `.env.example`) |
| Render / produção | Environment Variables / Secret Store do host |

Local (User Secrets):

```bash
dotnet user-secrets set "ConnectionStrings:CONNECTION_STRING" "Host=...;Database=...;Username=...;Password=..." --project ARP
dotnet user-secrets set "JWT_KEY" "sua-chave-longa-aleatoria" --project ARP
dotnet user-secrets set "KEY_HMAC" "outra-chave-longa-aleatoria" --project ARP
dotnet user-secrets set "JWT_EXPIRATION_HOURS" "8" --project ARP
dotnet user-secrets set "SMTP_USER" "noreply@seu-dominio.com" --project ARP
dotnet user-secrets set "SMTP_PASSWORD" "sua-senha-smtp" --project ARP
```

Ou via Visual Studio: botão direito no projeto `ARP` → **Manage User Secrets**.

Variáveis de ambiente equivalentes (Render/Docker): `CONNECTION_STRING`, `JWT_KEY`, `JWT_EXPIRATION_HOURS`, `KEY_HMAC`, `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_HOST`, `SMTP_PORT`, `CORS_ALLOWED_ORIGINS`.
## ☁️ ACESSOS

Credenciais de painéis (Render, Neon, e-mail/senha) **não** devem ser commitadas. Use o gerenciador de senhas / secret store do time.

## 🔑 TOKEN

Padrão JWT 

Por hora utiliza tempo de expiração de 8 horas, mas o correto é utilizar 15 minutos e utilizar o refresh tokne - Verificar;

![Padrão JWT](https://github.com/leomar-sartor/Mentant/blob/main/documentation/ModeloJWT.png)

## 🔎 LOGS

<p> Logs ainda não foram implementados; </p>

## 🔗 QUERYS E MUTATIONS (Insomnia)

A documentação já está disponivel no projeto, consulte: 

acesse /graphql ou /graphiql

Você pode utilizar as request do insomnia exportadas
[Aqui](https://github.com/leomar-sartor/Mentant/blob/main/documentation/Insomnia_Request.json). É só importar.

## 💯 Arquitetura

![Arquitetura](https://github.com/leomar-sartor/Mentant/blob/main/documentation/JBS.png)


## :shipit: Não gostou da documentação! Documenta o negócio aí e me ensina, assim eu apreendo com você!

# PADRÃO DE COMMITS

Ícone - tipo : descrição
🔑 - feat : crud de pessoas

# OBSERVAÇÕES FINAIS

1 - AUTH

* Token com expiração de 15m;
* Já existe RefreshToken;
* Já existe Logout;

2 - CADASTRO EMPRESA

* Apenas com Razão Social e Descrição
* Não entendi sobre tax_id, trade_name

3 - CADASTRO SETOR

* Auto Increment do EmpresaSetor - Rever

4 - PESSOA - EXEMPLO

* Cadastro de Pessoa simples para ter como base de teste ou estudo - Sem relacionamentos;

5 - OUTRAS

* Ainda será criado um endpoint para criar empresa com setor;
* Cors habilitado pra todos - depois rever;
* Terminar documentação
* Não coloquei o Authorize ainda;


QUESTIONARIO VERI
1 - A;
2 - C;
3 - C;
4 - B;
5 - A;
6 - C;
7 - C;
8 - C;