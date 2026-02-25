using ARP.Entity;
using ARP.Infra;
using ARP.Modules.Auth;
using ARP.Modules.Empresa;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

//builder.Services.AddPooledDbContextFactory<Context>(options =>
//    options.UseNpgsql(connection));


//Logs SQL
builder.Services.AddPooledDbContextFactory<Context>(options =>
{
    options.UseNpgsql(connection);

    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();

    options.LogTo(Console.WriteLine, LogLevel.Information);

    //Só SQL
    //options.LogTo(Console.WriteLine,
    //new[] { DbLoggerCategory.Database.Command.Name },
    //LogLevel.Information);
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

var log = app.Logger;

log.LogInformation($"LOG activate");

//if (app.Environment.IsDevelopment())

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

//dotnet ef migrations add InitialCreate --project ARP.Infra --startup-project ARP
//dotnet ef database update --project ARP.Infra --startup-project ARP
