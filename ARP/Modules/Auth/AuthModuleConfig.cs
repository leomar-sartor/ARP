using ARP.Service.Modules.Auth;
using HotChocolate.Execution.Configuration;

namespace ARP.Modules.Auth
{
    public static class AuthModuleConfig
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();

            return services;
        }
        public static IRequestExecutorBuilder AddAuthQueriesAndMutations(this IRequestExecutorBuilder builder)
        {
            builder.AddTypeExtension<AuthMutation>();
            builder.AddTypeExtension<AccessControlQuery>();
            builder.AddTypeExtension<AccessControlMutation>();

            return builder;
        }
    }
}
