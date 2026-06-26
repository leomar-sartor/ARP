using ARP.Entity.Cadastros;
using ARP.Filters;
using ARP.Infra;
using ARP.Infra.Jobs;
using ARP.Modules.Auth;
using ARP.Modules.Colaborador;
using ARP.Modules.Empresa;
using ARP.Modules.Job;
using ARP.Modules.Pesquisa;
using ARP.Modules.Pessoa;
using ARP.Modules.Setor;
using ARP.Service;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(
    opt => opt.AddPolicy("AllowAll", policy =>
        policy
            .WithOrigins("http://localhost:5173")
            //.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
        )
    );

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
    options.UseNpgsql(connection)
            
    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    //Esse NoTracking só diz ao EF Core: “por padrão, consultas não precisam ficar rastreadas pelo Change Tracker”. Para uma API GraphQL onde a maioria das queries só lê dados, isso costuma ser bom: usa menos memória e tende a ser mais rápido.

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
    .AddFiltering(descriptor =>
    {
        descriptor.AddDefaults();
        descriptor.Provider(
            new QueryableFilterProvider(x => x
                .AddFieldHandler<QueryableStringInvariantContainsHandler>()
                .AddDefaultFieldHandlers()
            )
        );
    })
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = true;
        opt.ExecutionTimeout = TimeSpan.FromMinutes(5);
    })
    .ModifyCostOptions(options =>
    {
        options.MaxFieldCost = 15000;       // seu fieldCost foi 2477 — coloque acima disso
        options.MaxTypeCost = 15000;
        options.EnforceCostLimits = true;  // mantém a proteção, só aumenta o limite
        options.ApplyCostDefaults = true;
        options.DefaultResolverCost = 10.0;
    })
    .ModifyOptions(o =>
    {
        o.DefaultResolverStrategy = ExecutionStrategy.Serial;
    })
    .AddProjections()
    .AddSorting()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddAuthorization()
    .AddAuthQueriesAndMutations()
    .AddEmpresaQueriesAndMutations()
    .AddSetorQueriesAndMutations()
    .AddPessoaQueriesAndMutations()
    .AddColaboradorQueriesAndMutations()
    .AddPesquisaQueriesAndMutations()
    .AddJobMutations()
    .DisableIntrospection(false);

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

builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<RefreshTokenCleanupJob>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RefreshTokenCleanupJob>());

var app = builder.Build();

var isDevelopment = app.Environment.IsDevelopment();

if (isDevelopment)
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

app.UseCors("AllowAll");

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