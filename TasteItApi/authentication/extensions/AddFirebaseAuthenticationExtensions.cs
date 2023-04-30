using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;

namespace TasteItApi.authentication.extensions
{
    public static class AddFirebaseAuthenticationExtensions
    {
        public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, (o) => { });

            services.AddScoped<FirebaseAuthenticationFunctionHandler>();

            return services;
        }
    }
}
