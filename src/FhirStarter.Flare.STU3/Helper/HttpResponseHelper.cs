using System.Net;
using System.Net.Http;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Flare.STU3.Helper
{
    public static class HttpResponseHelper
    {
        public static HttpResponseMessage ConvertResourceToHttpResponseMessage(Resource resource, HttpStatusCode statusCode)
        {
            var xml = new FhirXmlSerializer().SerializeToString(resource);
            var httpContent = new StringContent(xml, Encoding.UTF8, "application/xml");
            var response = new HttpResponseMessage(statusCode) { Content = httpContent };
            return response;
        }
    }
}
