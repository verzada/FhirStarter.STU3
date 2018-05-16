using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;

namespace FhirStarter.Inferno.IntegrationTests.Helpers
{
    public static class FhirClientHelper
    {
        public static List<FhirClient> GetFhirClients()
        {
            var serversStr = GetTestServer();
            var serviceName = GetServiceName();
            var listsOfFhirClient = new List<FhirClient>();

            var serversArray = serversStr.Split(",".ToCharArray());

            foreach (var server in serversArray)
            {
                var client = GetFhirClientEndpoint(server, serviceName);
                client.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultFhirTimeout"]);
                listsOfFhirClient.Add(client);
            }
            return listsOfFhirClient;
        }

        public static string GetServiceName()
        {
            var serviceName = ConfigurationManager.AppSettings["ServiceName"];
            return serviceName;
        }

        public static string GetTestServer()
        {
            var serversStr = ConfigurationManager.AppSettings["TestServers"];
            return serversStr;
        }

        private static FhirClient GetFhirClientEndpoint(string server, string serviceName)
        {
            var url = GetFhirClientUrl(server, serviceName);

            var client = new FhirClient(url);
            return client;
        }

        private static string GetFhirClientUrl(string server, string serviceName)
        {
            Console.WriteLine("Testing on " + server);

            var url = "http://" + server;

            if (!server.ToLower().Contains("localhost"))
            {
                url += "/" + serviceName;
            }
            url += "/fhir";
            url = url.Trim();

            Console.WriteLine("Current url is " + url);

            return url;
        }
    }
}
