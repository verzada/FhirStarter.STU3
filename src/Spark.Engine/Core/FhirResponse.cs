using System.Net;
using Hl7.Fhir.Model;

namespace Spark.Engine.Core
{
    public class FhirResponse
    {
        public HttpStatusCode StatusCode;
        public IKey Key;
        public Resource Resource;

        public FhirResponse(HttpStatusCode code)
        {
            StatusCode = code;
            Key = null;
            Resource = null;
        }

        public bool HasBody => Resource != null;
    }
}
