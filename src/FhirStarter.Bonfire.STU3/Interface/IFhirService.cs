using System.Collections.Generic;
using System.Net.Http;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Spark.Engine.Core;

namespace FhirStarter.Bonfire.STU3.Interface
{
    /// <summary>
    /// The interface used by the FhirStarter Inferno server to expose the fhir service. 
    /// It is not meant to handle internal services
    /// </summary>
    public interface IFhirService
    {

        // The name of the Resource you can query (earlier called GetAlias)
        string GetServiceResourceReference();

        CapabilityStatement.RestComponent GetRestDefinition();

        OperationDefinition GetOperationDefinition();

        // CRUD
        HttpResponseMessage Create(IKey key, Resource resource);
        Base Read(SearchParams searchParams);
        Base Read(string id);
        HttpResponseMessage Update(IKey key, Resource resource);
        HttpResponseMessage Delete(IKey key);

        HttpResponseMessage Patch(IKey key, Resource resource);
    }
}
