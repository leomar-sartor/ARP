using ARP.Entity;
using ARP.Infra;
using ARP.Modules.Auth;
using ARP.Modules.Empresa;
using ARP.Modules.Pessoa;
using ARP.Modules.Setor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthModule();

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration.GetConnectionString("JWT_KEY"); ;
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Environment variable 'JWT_KEY' is not set or is empty. Please set JWT_KEY.");

var key = Encoding.ASCII.GetBytes(jwtKey);

var connection = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException("Database connection string is not configured. Set 'CONNECTION_STRING' as an environment variable or in configuration.");

builder.Services
    .AddIdentity<Usuario, IdentityRole<long>>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
});

builder.Services.AddPooledDbContextFactory<Context>(options =>
{
    options.UseNpgsql(connection);

    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();

    options.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };
    });

builder.Services
    .AddGraphQLServer()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddAuthorization()
    .AddAuthQueriesAndMutations()
    .AddEmpresaQueriesAndMutations()
    .AddSetorQueriesAndMutations()
    .AddPessoaQueriesAndMutations();

builder.Logging.AddConsole(options =>
{
    options.FormatterName = "simple";
});

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    options.TimestampFormat = "HH:mm:ss ";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var log = app.Logger;
    log.LogInformation($"LOG EXAMPLE");
}

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedOptions);

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/graphql");

app.UseGraphQLGraphiQL("/graphiql");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Context>();
    db.Database.Migrate();
}

app.Run();