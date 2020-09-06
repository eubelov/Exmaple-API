using Microsoft.AspNetCore.Authorization;

namespace SampleAPI.Auth
{
    public class Policies
    {
        public const string Admin = "Admin";

        public const string User = "User";

        public const string AuthenticatedUser = "AnyAuthenticatedUser";

        public static AuthorizationPolicy AdminPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(Admin)
                .Build();
        }

        public static AuthorizationPolicy UserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(User)
                .Build();
        }

        public static AuthorizationPolicy AnyAuthenticatedUserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        }
    }
}