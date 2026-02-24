using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using DbUp;

namespace ARP.Infra.Extensions
{
    public static class DataBaseExtensions
    {
        public static IHost MigrateDatabase(this IHost host, ILogger logger, bool isDev = false)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();

                string? connection = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                if (string.IsNullOrEmpty(connection))
                    throw new InvalidOperationException("The connection string is not set in the environment variables.");

                EnsureDatabase.For.PostgresqlDatabase(connection);

                var assembly = Assembly.GetExecutingAssembly();

                var upgrader = DeployChanges.To
                    .PostgresqlDatabase(connection)
                    .WithScriptsEmbeddedInAssembly(assembly)
                    .LogToConsole()
                    .Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    var msg = result.Error.Message;
                    logger.LogInformation($"ERRO MIGRATION: {msg}");

                    throw new Exception($"Erro no rodar Migartion: ", result.Error);
                }
            }

            return host;
        }
    }
}
