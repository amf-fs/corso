using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CorsoApi.Infrastructure
{
    public class SessionAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var isAuthorized = httpContext.Session.GetString("IsAuthorized");

            if (string.IsNullOrWhiteSpace(isAuthorized) || isAuthorized != "true")
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

    public class SessionAuthorizeAttribute : TypeFilterAttribute
    {
        public SessionAuthorizeAttribute() : base(typeof(SessionAuthorizationFilter))
        {
        }
    }
}