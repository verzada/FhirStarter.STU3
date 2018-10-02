using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using FhirStarter.Flare.STU3.Log;
using log4net.Config;
using Spark.Engine.Extensions;

namespace FhirStarter.Flare.STU3
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            GlobalConfiguration.Configuration.MessageHandlers.Add(new MessageLoggingHandler());
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(Configure);
        }

        private void Configure(HttpConfiguration config)
        {
            config.AddFhir();
        }
    }
}
