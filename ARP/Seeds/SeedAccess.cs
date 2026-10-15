using ARP.Entity.Cadastros;
using ARP.Infra;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARP.Seeds;

public static class SeedAccess
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, bool isDevelopment)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedAccess");
        await using var transaction = await db.Database.BeginTransactionAsync();

        if (!await roles.RoleExistsAsync("Admin"))
            Ensure(await roles.CreateAsync(new IdentityRole<long>("Admin")));

        var adminEmail = configuration["GLOBAL_ADMIN_EMAIL"] ?? configuration["Seeds:AdminEmail"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var admin = await users.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                var password = configuration["Seeds:AdminPassword"];
                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Configure Seeds:AdminPassword para criar o administrador inicial.");
                admin = new Usuario { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, Cpf = "00000000000", Ativo = true };
                Ensure(await users.CreateAsync(admin, password));
            }
            if (!admin.Ativo)
            {
                admin.Ativo = true;
                Ensure(await users.UpdateAsync(admin));
            }
            if (!await users.IsInRoleAsync(admin, "Admin"))
                Ensure(await users.AddToRoleAsync(admin, "Admin"));
            logger.LogInformation("Seed: administrador global {Email} disponível", adminEmail);
        }

        if (isDevelopment && configuration.GetValue<bool>("Seeds:TestUsersEnabled"))
            await SeedTestUsersAsync(db, users, configuration);

        await transaction.CommitAsync();
        logger.LogInformation("Seeds de acesso concluídos; contas de teste habilitadas: {Enabled}",
            isDevelopment && configuration.GetValue<bool>("Seeds:TestUsersEnabled"));
    }

    private static async Task SeedTestUsersAsync(Context db, UserManager<Usuario> users, IConfiguration configuration)
    {
        var password = configuration["Seeds:TestPassword"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configure Seeds:TestPassword para criar as contas de teste.");
        var companyId = configuration.GetValue<long?>("Seeds:CompanyId");
        var company = companyId.HasValue
            ? await db.Empresas.Include(e => e.Tenant).SingleAsync(e => e.Id == companyId.Value && e.Ativo && e.Tenant.Ativo)
            : await db.Empresas.Include(e => e.Tenant).Where(e => e.Ativo && e.Tenant.Ativo).OrderBy(e => e.Id).FirstOrDefaultAsync();
        if (company is null)
        {
            var tenant = new Tenant { Nome = "Ambiente de testes", UserId = null };
            company = new Empresa { NomeFantasia = "Empresa de testes", Cnpj = "00000000000191", Tenant = tenant, UserId = null };
            db.Empresas.Add(company);
            await db.SaveChangesAsync();
        }

        foreach (var (code, cpf) in new[] { (PersonaCodes.Operator, "00000000001"), (PersonaCodes.Company, "00000000002"), (PersonaCodes.Collaborator, "00000000003") })
        {
            var email = $"{code}@arp.test";
            var user = await users.FindByEmailAsync(email);
            if (user is null)
            {
                user = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Cpf = cpf, Ativo = true, EmpresaId = company.Id };
                Ensure(await users.CreateAsync(user, password));
            }
            else if (!user.Ativo)
            {
                user.Ativo = true;
                Ensure(await users.UpdateAsync(user));
            }
            // Keep access changes made by Admin when the development seed runs again.
            var tenantLink = await db.UsuariosTenants.IgnoreQueryFilters().SingleOrDefaultAsync(m => m.UsuarioId == user.Id && m.TenantId == company.TenantId);
            if (tenantLink is null) db.UsuariosTenants.Add(new UsuarioTenant { UsuarioId = user.Id, TenantId = company.TenantId, UserId = null });
            var companyLink = await db.UsuariosEmpresas.IgnoreQueryFilters().SingleOrDefaultAsync(m => m.UsuarioId == user.Id && m.EmpresaId == company.Id);
            if (companyLink is null) db.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = user.Id, EmpresaId = company.Id, UserId = null });
            var personaId = await db.Personas.Where(p => p.Codigo == code && p.Ativo).Select(p => p.Id).SingleAsync();
            var assignment = await db.UsuariosPersonas.IgnoreQueryFilters().SingleOrDefaultAsync(a => a.UsuarioId == user.Id && a.PersonaId == personaId && a.TenantId == company.TenantId && a.EmpresaId == company.Id);
            if (assignment is null) db.UsuariosPersonas.Add(new UsuarioPersona { UsuarioId = user.Id, PersonaId = personaId, TenantId = company.TenantId, EmpresaId = company.Id, UserId = null });
            await db.SaveChangesAsync();

            if (code == PersonaCodes.Operator)
                await SeedSecondOperatorTenantAsync(db, user, personaId);

            if (code == PersonaCodes.Collaborator)
            {
                var collaborator = await db.Colaboradores.IgnoreQueryFilters().SingleOrDefaultAsync(c => c.UsuarioId == user.Id);
                var sector = await db.Setores.Where(s => s.EmpresaId == company.Id && s.Ativo).OrderBy(s => s.Id).FirstOrDefaultAsync();
                if (sector is null)
                {
                    sector = new Setor { Nome = "Testes de personas", EmpresaId = company.Id, UserId = null };
                    db.Setores.Add(sector);
                    await db.SaveChangesAsync();
                }
                if (collaborator is null)
                {
                    collaborator = new Colaborador { Nome = "Colaborador de teste", Email = email, Cpf = cpf, UsuarioId = user.Id, EmpresaId = company.Id, SetorId = sector.Id, UserId = null };
                    db.Colaboradores.Add(collaborator);
                    await db.SaveChangesAsync();
                }
                else { collaborator.Ativo = true; collaborator.DeletedAt = null; }
                var link = await db.ColaboradorEmpresas.IgnoreQueryFilters().SingleOrDefaultAsync(m => m.ColaboradorId == collaborator.Id && m.EmpresaId == company.Id);
                if (link is null) db.ColaboradorEmpresas.Add(new ColaboradorEmpresa { ColaboradorId = collaborator.Id, EmpresaId = company.Id, SetorId = sector.Id, UserId = null });
                else { link.Ativo = true; link.DeletedAt = null; }
                await db.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedSecondOperatorTenantAsync(Context db, Usuario user, long personaId)
    {
        const string cnpj = "00000000000272";
        var company = await db.Empresas.SingleOrDefaultAsync(e => e.Cnpj == cnpj);
        if (company is null)
        {
            company = new Empresa { Cnpj = cnpj, NomeFantasia = "Empresa de testes 2", Tenant = new Tenant { Nome = "Tenant de testes 2", UserId = null }, UserId = null };
            db.Empresas.Add(company);
            await db.SaveChangesAsync();
        }
        if (!await db.UsuariosTenants.AnyAsync(m => m.UsuarioId == user.Id && m.TenantId == company.TenantId))
            db.UsuariosTenants.Add(new UsuarioTenant { UsuarioId = user.Id, TenantId = company.TenantId, UserId = null });
        if (!await db.UsuariosEmpresas.AnyAsync(m => m.UsuarioId == user.Id && m.EmpresaId == company.Id))
            db.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = user.Id, EmpresaId = company.Id, UserId = null });
        if (!await db.UsuariosPersonas.AnyAsync(m => m.UsuarioId == user.Id && m.PersonaId == personaId && m.EmpresaId == company.Id))
            db.UsuariosPersonas.Add(new UsuarioPersona { UsuarioId = user.Id, PersonaId = personaId, TenantId = company.TenantId, EmpresaId = company.Id, UserId = null });
        var sector = await db.Setores.FirstOrDefaultAsync(s => s.EmpresaId == company.Id);
        if (sector is null)
        {
            sector = new Setor { Nome = "Operação de testes 2", EmpresaId = company.Id, UserId = null };
            db.Setores.Add(sector);
            await db.SaveChangesAsync();
        }
        if (!await db.Colaboradores.AnyAsync(c => c.Cpf == "00000000004"))
            db.Colaboradores.Add(new Colaborador { Nome = "Colaborador do tenant de testes 2", Cpf = "00000000004", Email = "colaborador2@arp.test", EmpresaId = company.Id, SetorId = sector.Id, UserId = null,
                EmpresasVinculadas = { new ColaboradorEmpresa { EmpresaId = company.Id, SetorId = sector.Id, UserId = null } } });
        await db.SaveChangesAsync();
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Falha no seed de acesso: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
