using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ARP.Infra.Jobs
{
    public class RefreshTokenCleanupJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupJob> _logger;

        // Roda uma vez por dia
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public RefreshTokenCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<RefreshTokenCleanupJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await CleanupAsync(ct);
                await Task.Delay(_interval, ct);
            }
        }

        public async Task CleanupAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();

            var cutoff = DateTime.UtcNow;

            // Remove tokens que satisfazem QUALQUER condição:
            // - Expirados (independente de revogado ou não)
            // - Revogados há mais de 3 dias (mantém histórico recente para auditoria)
            var deleted = await db.RefreshTokens
                .Where(rt =>
                    rt.Expiration < cutoff ||
                    (rt.RevokedAt != null && rt.RevokedAt < cutoff.AddDays(-3)))
                .ExecuteDeleteAsync(ct); // EF Core 7+ — delete direto no banco sem carregar em memória

            _logger.LogInformation(
                "Cleanup de RefreshTokens: {Count} tokens removidos em {Time}",
                deleted, DateTime.UtcNow);
        }
    }
}
