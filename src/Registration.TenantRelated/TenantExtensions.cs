using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Xylab.Tenant.Services;

namespace Xylab.Contesting.Registration
{
    public static class TenantExtensions
    {
        public static IServiceCollection AddContestRegistrationTenant(this IServiceCollection services)
        {
            services.ConfigureOptions<TenantConfigurator>();
            return services;
        }

        private class TenantConfigurator : IConfigureOptions<ContestRegistrationOptions>
        {
            private readonly IServiceProvider _serviceProvider;

            public TenantConfigurator(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public void Configure(ContestRegistrationOptions options)
            {
                options.Add(new TeachingClassRegisterProvider());
                options.Add(new StudentSelfRegisterProvider());

                using var scope = _serviceProvider.CreateScope();
                if (scope.ServiceProvider.GetService(typeof(IGroupStore)) != null)
                {
                    options.Add(new TrainingTeamRegisterProvider("team", 1));
                    options.Add(new TrainingTeamRegisterProvider("team-verify", 0));
                }
            }
        }

        public static bool TryGetStudent(
            this RegisterProviderContext context,
            out int affiliationId,
            out string? studentId,
            out string? affiliationName,
            out string? studentName)
        {
            affiliationId = int.MinValue;
            studentId = context.User.FindFirst("student")?.Value;
            affiliationName = context.User.FindFirst("affiliation")?.Value;
            studentName = context.User.FindFirst("given_name")?.Value;

            return studentId != null
                && affiliationName != null
                && studentName != null
                && int.TryParse(context.User.FindFirst("tenant")?.Value, out affiliationId);
        }
    }
}
