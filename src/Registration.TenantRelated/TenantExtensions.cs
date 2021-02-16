using Microsoft.Extensions.DependencyInjection;

namespace Ccs.Registration
{
    public static class TenantExtensions
    {
        public static IServiceCollection AddContestTenantStaff(this IServiceCollection services)
        {
            services.AddContestRegistrationProvider<TeachingClassRegisterProvider>();
            services.AddContestRegistrationProvider<StudentSelfRegisterProvider>();
            return services;
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
