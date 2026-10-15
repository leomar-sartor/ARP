using ARP.Infra.Jobs;
using ARP.Infra;
using ARP.Modules.Auth;

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
        [Service] Context context,
        [Service] TenantContextResolver tenantContextResolver,
        [Service] PermissionAuthorizationService permissionAuthorizationService,
        CancellationToken ct)
        {
            var activeContext = await tenantContextResolver.ResolveAsync(context, ct);
            await permissionAuthorizationService.RequireAdminAsync(activeContext);
            await job.CleanupAsync(ct);

            return true;
        }
    }
}
