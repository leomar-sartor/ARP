using ARP.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ARP.Infra;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<Usuario, IdentityRole<long>, long>(options)
{
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<EmpresaSetor> EmpresaSetores => Set<EmpresaSetor>();

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
    }

    private static LambdaExpression GenerateFilterExpression(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(Base.DeletedAt));
        var body = Expression.Equal(prop, Expression.Constant(null));
        return Expression.Lambda(body, param);
    }
}