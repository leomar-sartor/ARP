using ARP.Infra;
using ARP.Infra.Extensions;
using ARP.Infra.Interfaces;
using ARP.Modules.Auth;
using ARP.Modules.Empresa;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IConexao, Conexao>();

builder.Services.AddAuthModule();

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Environment variable 'JWT_KEY' is not set or is empty. Please set JWT_KEY.");

var key = Encoding.ASCII.GetBytes(jwtKey);

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
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddAuthorization()
    .AddAuthQueriesAndMutations()
    .AddEmpresaQueriesAndMutations()
    .AddFiltering()
    .AddSorting();

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

var log = app.Logger;

log.LogInformation($"LOG activate");

if (app.Environment.IsDevelopment())
    app.MigrateDatabase(log, true);

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/graphql");

app.UseGraphQLGraphiQL("/graphiql");

app.Run();

//https://chillicream.com/docs/hotchocolate/v13/defining-a-schema/object-types
//https://fiyazhasan.work/tag/graphql/page/2/
//https://github.com/fiyazbinhasan/GraphQLCoreFromScratch

