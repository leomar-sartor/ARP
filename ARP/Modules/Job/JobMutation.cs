using ARP.Infra.Jobs;

namespace ARP.Modules.Job
{
    [ExtendObjectType("Mutation")]
    public class JobMutation
    {
        private readonly ILogger<JobMutation> _logger;

        public JobMutation(
            ILogger<JobMutation> logger
            )
        {
            _logger = logger;
        }

        [GraphQLDescription("Limpar refreshTokens expirados")]
        public async Task<bool> RunRefreshTokenCleanupAsync(
        [Service] RefreshTokenCleanupJob job,
        CancellationToken ct)
        {
            await job.CleanupAsync(ct);

            return true;
        }
    }
}
