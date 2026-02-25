using Microsoft.EntityFrameworkCore;
using ARP.Entity;

namespace ARP.Infra;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

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
    }
}