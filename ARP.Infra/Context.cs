using ARP.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARP.Infra;

public class Context : IdentityDbContext<Usuario, IdentityRole<long>, long>
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Setor> Setores => Set<Setor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RazaoSocial)
                .IsRequired()
                .HasMaxLength(200);
        });

        //Muitos pra Muitos
        modelBuilder.Entity<Empresa>()
           .HasMany(e => e.Setores)
           .WithMany(s => s.Empresas)
           .UsingEntity(j => j.ToTable("EmpresaSetor"));

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);
    }
}