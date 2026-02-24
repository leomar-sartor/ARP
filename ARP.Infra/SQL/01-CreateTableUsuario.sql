CREATE TABLE Log (
   Id SERIAL PRIMARY KEY NOT NULL,
   UserId int8,
   Entidade VARCHAR NOT null,
   EntidadeId int8 NOT null,
   Operacao VARCHAR NOT null,
   Campos TEXT NOT NULL,
   Mensagem TEXT,
   CreatedAt timestamp NOT NULL
);

CREATE TABLE Usuario (
   Id SERIAL PRIMARY KEY NOT NULL,
   Cpf VARCHAR NOT NULL,
   Email VARCHAR NOT NULL,
   UserName VARCHAR NOT NULL,
   Password VARCHAR NOT NULL,
   CreatedAt timestamp NOT NULL,
   UpdatedAt timestamp NOT NULL,
   DeletedAt timestamp
);

CREATE TABLE Empresa (
   Id SERIAL PRIMARY KEY NOT NULL,
   RazaoSocial VARCHAR NOT NULL,
   Descricao VARCHAR NOT NULL,
   CreatedAt timestamp NOT NULL,
   UpdatedAt timestamp NOT NULL,
   DeletedAt timestamp
);

CREATE TABLE Setor (
   Id SERIAL PRIMARY KEY NOT NULL,
   Nome VARCHAR NOT NULL,
   Descricao VARCHAR NOT NULL,
   CreatedAt timestamp NOT NULL,
   UpdatedAt timestamp NOT NULL,
   DeletedAt timestamp
);