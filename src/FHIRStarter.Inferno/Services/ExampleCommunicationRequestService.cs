using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using FhirStarter.Bonfire.STU3.Interface;
using FhirStarter.Flare.STU3.Helper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;

namespace FhirStarter.Inferno.Services
{
    public class ExampleCommunicationRequestService:IFhirService
    {
        public string GetServiceResourceReference()
        {
            return nameof(CommunicationRequest);
        }

        public HttpResponseMessage Create(IKey key, Resource resource)
        {
            var request = (CommunicationRequest)resource;
            var xmlAsString = new FhirXmlSerializer().SerializeToString(request);
            var result = new FhirXmlParser().Parse<CommunicationRequest>(xmlAsString);
            result.Id = Guid.NewGuid().ToString();
            return HttpResponseHelper.ConvertResourceToHttpResponseMessage(result, HttpStatusCode.OK);

        }

        public Base Read(SearchParams searchParams)
        {
            throw new NotImplementedException();
        }

        public Base Read(string id)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage Update(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage Delete(IKey key)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage Patch(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public CapabilityStatement.RestComponent GetRestDefinition()
        {
           return new CapabilityStatement.RestComponent();
        }

        public OperationDefinition GetOperationDefinition()
        {
            return new OperationDefinition();
        }

        public ICollection<string> GetStructureDefinitionNames()
        {
            return new List<string> { GetServiceResourceReference() };
        }
    }
}