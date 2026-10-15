using ARP.Entity;
using ARP.Entity.Cadastros;
using ARP.Entity.Exemplo;
using ARP.Entity.Pesquisas;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ARP.Infra;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<Usuario, IdentityRole<long>, long>(options)
{
    #region Example Pessoas
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Habilidade> Habilidades => Set<Habilidade>();
    public DbSet<PessoaHabilidade> PessoaHabilidades => Set<PessoaHabilidade>();
    #endregion

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    #region Cadastros
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<UsuarioTenant> UsuariosTenants => Set<UsuarioTenant>();
    public DbSet<UsuarioEmpresa> UsuariosEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PersonaPermissao> PersonasPermissoes => Set<PersonaPermissao>();
    public DbSet<UsuarioPersona> UsuariosPersonas => Set<UsuarioPersona>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<ColaboradorEmpresa> ColaboradorEmpresas => Set<ColaboradorEmpresa>();
    #endregion

    #region Pesquisas
    public DbSet<Convite> Convites => Set<Convite>();
    public DbSet<Pesquisa> Pesquisas => Set<Pesquisa>();
    public DbSet<PesquisaEmpresa> PesquisaEmpresas => Set<PesquisaEmpresa>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Questao> Questoes => Set<Questao>();
    public DbSet<QuestaoOpcao> QuestaoOpcoes => Set<QuestaoOpcao>();
    public DbSet<QuestaoResposta> QuestaoRespostas => Set<QuestaoResposta>();
    public DbSet<PesquisaRascunho> PesquisaRascunhos => Set<PesquisaRascunho>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete global
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Base).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(
                        GenerateFilterExpression(entityType.ClrType));
            }
        }

        #region Cadastros
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Empresa>()
            .HasOne(e => e.Tenant)
            .WithMany(t => t.Empresas)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empresa>()
            .HasAlternateKey(e => new { e.TenantId, e.Id });

        modelBuilder.Entity<UsuarioTenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UsuarioId, e.TenantId }).IsUnique();
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.TenantsVinculados)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.UsuariosVinculados)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UsuarioId, e.EmpresaId }).IsUnique();
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.EmpresasVinculadas)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.UsuariosVinculados)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        modelBuilder.Entity<Permissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        modelBuilder.Entity<PersonaPermissao>(entity =>
        {
            entity.HasKey(e => new { e.PersonaId, e.PermissaoId });
            entity.HasOne(e => e.Persona)
                .WithMany(p => p.Permissoes)
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Permissao)
                .WithMany(p => p.Personas)
                .HasForeignKey(e => e.PermissaoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioPersona>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UsuarioId, e.PersonaId, e.TenantId })
                .HasFilter("\"EmpresaId\" IS NULL")
                .IsUnique();
            entity.HasIndex(e => new { e.UsuarioId, e.PersonaId, e.TenantId, e.EmpresaId })
                .HasFilter("\"EmpresaId\" IS NOT NULL")
                .IsUnique();
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.PersonasAtribuidas)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Persona)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.PersonasAtribuidas)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.UsuariosPersonas)
                .HasForeignKey(e => new { e.TenantId, e.EmpresaId })
                .HasPrincipalKey(e => new { e.TenantId, e.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        var seedTimestamp = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Persona>().HasData(
            new Persona { Id = 1, Codigo = PersonaCodes.Operator, Nome = "Operador", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Persona { Id = 2, Codigo = PersonaCodes.Collaborator, Nome = "Colaborador", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Persona { Id = 3, Codigo = PersonaCodes.Company, Nome = "Empresa", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Persona { Id = 4, Codigo = PersonaCodes.Admin, Nome = "Admin", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null });

        modelBuilder.Entity<Permissao>().HasData(
            new Permissao { Id = 1, Codigo = PermissionCodes.CompanyView, Descricao = "Visualizar empresas autorizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 2, Codigo = PermissionCodes.CompanyCreate, Descricao = "Cadastrar empresas no tenant ativo", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 3, Codigo = PermissionCodes.CompanyEdit, Descricao = "Editar empresas autorizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 4, Codigo = PermissionCodes.CompanyManage, Descricao = "Gerenciar empresas autorizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 5, Codigo = PermissionCodes.CollaboratorView, Descricao = "Visualizar colaboradores autorizados", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 6, Codigo = PermissionCodes.CollaboratorManage, Descricao = "Gerenciar colaboradores autorizados", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 7, Codigo = PermissionCodes.StandardSurveyCreate, Descricao = "Criar pesquisas padronizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 8, Codigo = PermissionCodes.StandardSurveyEdit, Descricao = "Editar pesquisas padronizadas não publicadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 9, Codigo = PermissionCodes.StandardSurveyPublish, Descricao = "Publicar pesquisas padronizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 10, Codigo = PermissionCodes.StandardSurveyManage, Descricao = "Gerenciar pesquisas padronizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 11, Codigo = PermissionCodes.SurveyRespond, Descricao = "Responder pesquisas atribuídas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 12, Codigo = PermissionCodes.SurveyTrack, Descricao = "Acompanhar indicadores operacionais", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 13, Codigo = PermissionCodes.TenantSwitch, Descricao = "Alternar entre tenants autorizados", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 14, Codigo = PermissionCodes.PermissionManage, Descricao = "Gerenciar personas e permissões", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 15, Codigo = PermissionCodes.SectorManage, Descricao = "Gerenciar setores autorizados", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null },
            new Permissao { Id = 16, Codigo = PermissionCodes.CategoryManage, Descricao = "Gerenciar categorias de pesquisas padronizadas", Ativo = true, UserId = null, CreatedAt = seedTimestamp, UpdatedAt = seedTimestamp, DeletedAt = null });

        modelBuilder.Entity<PersonaPermissao>().HasData(
            new PersonaPermissao { PersonaId = 1, PermissaoId = 1 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 2 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 3 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 4 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 5 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 6 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 7 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 8 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 9 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 10 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 12 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 13 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 15 },
            new PersonaPermissao { PersonaId = 1, PermissaoId = 16 },
            new PersonaPermissao { PersonaId = 2, PermissaoId = 11 },
            new PersonaPermissao { PersonaId = 2, PermissaoId = 13 },
            new PersonaPermissao { PersonaId = 3, PermissaoId = 1 },
            new PersonaPermissao { PersonaId = 3, PermissaoId = 3 },
            new PersonaPermissao { PersonaId = 3, PermissaoId = 12 },
            new PersonaPermissao { PersonaId = 3, PermissaoId = 13 });

        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.Cnpj)
            .IsUnique();

        modelBuilder.Entity<Setor>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Setor>()
            .HasOne(e => e.Empresa)
            .WithMany(p => p.Setores)
            .HasForeignKey(e => e.EmpresaId);

        modelBuilder.Entity<Setor>()
            .HasAlternateKey(e => new { e.EmpresaId, e.Id });

        modelBuilder.Entity<Colaborador>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Colaborador>()
            .HasIndex(c => c.Cpf)
            .IsUnique();

        modelBuilder.Entity<Colaborador>()
            .HasIndex(c => c.UsuarioId)
            .IsUnique();

        modelBuilder.Entity<Colaborador>()
            .HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Colaborador>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Colaborador>()
            .HasOne(e => e.Setor)
            .WithMany(p => p.Colaboradores)
            .HasForeignKey(e => e.SetorId);

        modelBuilder.Entity<Colaborador>()
            .HasOne(e => e.Empresa)
            .WithMany(p => p.Colaboradores)
            .HasForeignKey(e => e.EmpresaId);

        modelBuilder.Entity<ColaboradorEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ColaboradorId, e.EmpresaId }).IsUnique();
            entity.HasOne(e => e.Colaborador)
                .WithMany(c => c.EmpresasVinculadas)
                .HasForeignKey(e => e.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.ColaboradoresVinculados)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Setor)
                .WithMany(s => s.ColaboradoresVinculados)
                .HasForeignKey(e => new { e.EmpresaId, e.SetorId })
                .HasPrincipalKey(s => new { s.EmpresaId, s.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });
        #endregion

        #region Identity
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

        modelBuilder.Entity<Usuario>().ToTable("arp_user");
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Cpf)
            .IsUnique();
        modelBuilder.Entity<IdentityRole<long>>().ToTable("arp_role");
        modelBuilder.Entity<IdentityUserRole<long>>().ToTable("arp_userrole")
            .HasKey(r => new { r.UserId, r.RoleId });
        modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("arp_userclaim")
            .HasKey(r => new { r.Id });
        modelBuilder.Entity<IdentityUserToken<long>>().ToTable("arp_usertoken");
        modelBuilder.Entity<IdentityRoleClaim<long>>().ToTable("arp_roleclaim");
        #endregion

        #region Exemplo Pessoa
        //Relacionamento 1:N Example
        modelBuilder.Entity<Endereco>()
        .HasOne(e => e.Pessoa)
        .WithMany(p => p.Enderecos)
        .HasForeignKey(e => e.PessoaId);

        //Garante que dados não dupliquem - Índice único
        modelBuilder.Entity<Habilidade>()
            .HasIndex(h => h.Nome)
            .IsUnique();

        //Relacionamento N:M Example
        modelBuilder.Entity<PessoaHabilidade>()
            .HasKey(ph => new { ph.PessoaId, ph.HabilidadeId });

        modelBuilder.Entity<PessoaHabilidade>()
            .HasOne(ph => ph.Pessoa)
            .WithMany(p => p.PessoaHabilidades)
         .HasForeignKey(ph => ph.PessoaId);

        modelBuilder.Entity<PessoaHabilidade>()
            .HasOne(ph => ph.Habilidade)
            .WithMany(h => h.PessoaHabilidades)
            .HasForeignKey(ph => ph.HabilidadeId);
        #endregion

        #region Pesquisas

        modelBuilder.Entity<Convite>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Convite>()
            .HasIndex(x => x.Token).IsUnique();

        modelBuilder.Entity<Pesquisa>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Pesquisa>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PesquisaEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PesquisaId, e.EmpresaId }).IsUnique();
            entity.HasOne(e => e.Pesquisa)
                .WithMany(p => p.EmpresasVinculadas)
                .HasForeignKey(e => e.PesquisaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => new { e.TenantId, e.EmpresaId })
                .HasPrincipalKey(e => new { e.TenantId, e.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Questao>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<QuestaoOpcao>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<QuestaoResposta>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Convite>()
           .HasOne(e => e.Pesquisa)
           .WithMany(p => p.Convites)
           .HasForeignKey(e => e.PesquisaId);

        modelBuilder.Entity<Convite>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Questao>()
          .HasOne(e => e.Pesquisa)
          .WithMany(p => p.Questoes)
          .HasForeignKey(e => e.PesquisaId);

        modelBuilder.Entity<Questao>()
          .HasOne(e => e.Categoria)
          .WithMany(c => c.Questoes)
          .HasForeignKey(e => e.CategoriaId);

        modelBuilder.Entity<QuestaoOpcao>()
           .HasOne(q => q.Questao)
           .WithMany(q => q.Opcoes)
           .HasForeignKey(q => q.QuestaoId);

        modelBuilder.Entity<QuestaoResposta>()
            .HasOne(x => x.Questao)
            .WithMany(x => x.Respostas)
            .HasForeignKey(x => x.QuestaoId);

        modelBuilder.Entity<QuestaoResposta>()
            .HasOne(x => x.QuestaoOpcao)
            .WithMany()
            .HasForeignKey(x => x.QuestaoOpcaoId);

        modelBuilder.Entity<PesquisaRascunho>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        modelBuilder.Entity<PesquisaRascunho>()
            .HasOne(e => e.Pesquisa)
            .WithMany()
            .HasForeignKey(e => e.PesquisaId);
        #endregion
    }

    private static LambdaExpression GenerateFilterExpression(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(Base.DeletedAt));
        var body = Expression.Equal(prop, Expression.Constant(null));
        return Expression.Lambda(body, param);
    }
}
