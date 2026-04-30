using ARP.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ARP.Infra;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<Usuario, IdentityRole<long>, long>(options)
{
    #region Example
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Habilidade> Habilidades => Set<Habilidade>();
    public DbSet<PessoaHabilidade> PessoaHabilidades => Set<PessoaHabilidade>();
    #endregion

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<EmpresaSetor> EmpresaSetores => Set<EmpresaSetor>();

    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();


    public DbSet<Convite> Convites => Set<Convite>();
    public DbSet<Pesquisa> Pesquisas => Set<Pesquisa>();
    public DbSet<Questao> Questoes => Set<Questao>();
    public DbSet<QuestaoOpcao> QuestaoOpcoes => Set<QuestaoOpcao>();
    public DbSet<QuestaoResposta> QuestaoRespostas => Set<QuestaoResposta>();

    public DbSet<PesquisaRascunho> PesquisaRascunhos => Set<PesquisaRascunho>();

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

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RazaoSocial)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<EmpresaSetor>()
        .HasKey(x => new { x.EmpresaId, x.SetorId });

        modelBuilder.Entity<EmpresaSetor>()
            .HasOne(x => x.Empresa)
            .WithMany(e => e.EmpresaSetores)
            .HasForeignKey(x => x.EmpresaId);

        modelBuilder.Entity<EmpresaSetor>()
            .HasOne(x => x.Setor)
            .WithMany(s => s.EmpresaSetores)
            .HasForeignKey(x => x.SetorId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

        //modelBuilder.Entity<Usuario>().ToTable("arp_user");
        //modelBuilder.Entity<IdentityRole<long>>().ToTable("arp_role");
        //modelBuilder.Entity<IdentityUserRole<long>>().ToTable("arp_userrole")
        //    .HasKey(r => new { r.UserId, r.RoleId });
        //modelBuilder.Entity<IdentityUserClaim<long>>().ToTable("arp_userclaim")
        //    .HasKey(r => new { r.Id });
        //modelBuilder.Entity<IdentityUserToken<long>>().ToTable("arp_usertoken");
        //modelBuilder.Entity<IdentityRoleClaim<long>>().ToTable("arp_roleclaim");
        //modelBuilder.Entity<IdentityUserToken<long>>().ToTable("arp_usertoken");

        #region Example
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


        modelBuilder.Entity<Colaborador>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Colaborador>()
        .HasOne(e => e.Setor)
        .WithMany(p => p.Colaboradores)
        .HasForeignKey(e => e.SetorId);

        modelBuilder.Entity<Colaborador>()
        .HasOne(e => e.Empresa)
        .WithMany(p => p.Colaboradores)
        .HasForeignKey(e => e.EmpresaId);

        //Pesquisa

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

        modelBuilder.Entity<Questao>()
          .HasOne(e => e.Pesquisa)
          .WithMany(p => p.Questoes)
          .HasForeignKey(e => e.PesquisaId);

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
    }

    private static LambdaExpression GenerateFilterExpression(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(Base.DeletedAt));
        var body = Expression.Equal(prop, Expression.Constant(null));
        return Expression.Lambda(body, param);
    }
}