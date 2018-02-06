using System.Net.Http;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Spark.Engine.Core;

namespace FhirStarter.Inferno.Services
{
    public class ExamplePatientMockupService:IFhirMockupService
    {
        public string GetServiceResourceReference()
        {
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Create(IKey key, Resource resource)
        {
            throw new System.NotImplementedException();
        }

        public Base Read(SearchParams searchParams)
        {
            throw new System.NotImplementedException();
        }

        public Base Read(string id)
        {
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Update(IKey key, Resource resource)
        {
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Delete(IKey key)
        {
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Patch(IKey key, Resource resource)
        {
            throw new System.NotImplementedException();
        }
    }
}