using System.Collections.Generic;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.InMemory;
using Owin;

[assembly: OwinStartup(typeof(Apis.Startup))]
namespace FhirStarter.Inferno.App_Start
{
    public class StartupIdentityServer
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new IdentityServerOptions
            {
                Factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Clients.Get())
                    .UseInMemoryScopes(Scopes.Get())
                    .UseInMemoryUsers(new List<InMemoryUser>()),

                RequireSsl = false
            };

            app.UseIdentityServer(options);
        }
    }
}