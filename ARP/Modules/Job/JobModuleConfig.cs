using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Job
{
    public static class JobModuleConfig
    {
        public static IRequestExecutorBuilder AddJobMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<JobMutation>();
            return builder;
        }
    }
}
