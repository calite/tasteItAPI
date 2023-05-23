using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TasteItApi.authentication
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthByProfile : Attribute, IAuthorizationFilter
    {
        private readonly string[] RolesAllowed;

        public AuthByProfile(string[] rolesAllowed)
        {
            RolesAllowed = rolesAllowed;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            bool isAllowed = false;

            foreach (var rol in RolesAllowed)
            {
                if (rol == user.Claims.FirstOrDefault(r => r.Type == "profile")?.Value)
                {
                    isAllowed = true;
                }
            }

            if (!user.Identity.IsAuthenticated || !isAllowed)
            {
                context.Result = new UnauthorizedResult();
            }

        }
    }
    
}
