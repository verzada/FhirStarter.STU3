using System.Web.Http;

namespace FhirStarter.Flare.STU3
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.EnableSystemDiagnosticsTracing();
            config.EnsureInitialized();

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}
