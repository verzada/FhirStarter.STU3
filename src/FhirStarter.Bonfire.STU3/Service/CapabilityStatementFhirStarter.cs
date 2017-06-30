using System.Collections.Generic;
using System.Linq;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Spark.Engine.Service.FhirServiceExtensions;

namespace FhirStarter.Bonfire.STU3.Service
{
    public static class CapabilityStatementFhirStarter
    {
        public static CapabilityStatement AddUsedResources(this CapabilityStatement capabilityStatement,
    IEnumerable<IFhirService> services, bool readhistory, bool updatecreate,
    CapabilityStatement.ResourceVersionPolicy versioning)
        {
            var totalAvailableResources = new List<string>();
            foreach (var service in services)
            {
                var resourcesService = service;
                if (resourcesService != null)
                {
                    totalAvailableResources.AddRange(resourcesService.GetSupportedResources());
                }
            }

            foreach (var resource in totalAvailableResources)
            {
                capabilityStatement.AddSingleResourceComponent(resource, readhistory, updatecreate, versioning);
            }
            return capabilityStatement;
        }

        public static CapabilityStatement.ResourceComponent AddCoreSearchParamsResource(CapabilityStatement.ResourceComponent r,
          IEnumerable<ModelInfo.SearchParamDefinition> availableModelInfo)
        {
            if (availableModelInfo != null)
            {
                var parameters =
                    availableModelInfo.Where(sp => sp.Resource == r.Type.GetLiteral())
                        .Select(sp => new CapabilityStatement.SearchParamComponent
                        {
                            Name = sp.Name,
                            Type = sp.Type,
                            Documentation = sp.Description
                        });

                r.SearchParam.AddRange(parameters);
            }
            return r;
        }

        public static CapabilityStatement AddCoreSearchParamsAllResources(this CapabilityStatement capabilityStatement,
          IEnumerable<IFhirService> services)
        {
            var fhirStarterServices = services as IFhirService[] ?? services.ToArray();
            var firstOrDefault = capabilityStatement.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var r in firstOrDefault.Resource.ToList())
                {
                    foreach (var service in fhirStarterServices)
                    {
                        var resourceService = service;
                        if (resourceService != null)
                        {
                            //capabilityStatement.Rest().Resource.Remove(r);                            
                            //capabilityStatement.Rest().Resource.Add(resourceService.CreateResource());
                            capabilityStatement.Rest.Add(resourceService.GetRestDefinition());
                        }
                    }
                }
            return capabilityStatement;
        }

        public static CapabilityStatement AddOperationDefintion(this CapabilityStatement conformance, IEnumerable<IFhirService> services)
        {
            var operationComponents = new List<CapabilityStatement.OperationComponent>();

            foreach (var service in services)
            {
                var queryService = service;
                var operationDefintion = queryService?.GetOperationDefinition();
                if (!string.IsNullOrEmpty(operationDefintion?.Url))
                    operationComponents.Add(new CapabilityStatement.OperationComponent
                    {
                        Name = operationDefintion.Name,
                        Definition = new ResourceReference { Reference = operationDefintion.Url }
                    });
            }
            if (operationComponents.Count > 0)
                conformance.Server().Operation.AddRange(operationComponents);
            return conformance;
        }
    }
}
