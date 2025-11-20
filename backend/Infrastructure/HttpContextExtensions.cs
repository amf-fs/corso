namespace CorsoApi.Infrastructure
{
    public static class HttpContextExtensions
    {
        public static void CreateAuthenticationSession(this HttpContext httpContext)
        {
            httpContext.Session.Clear();
            httpContext.Session.SetString("IsAuthorized", "true");
        }
    }
}