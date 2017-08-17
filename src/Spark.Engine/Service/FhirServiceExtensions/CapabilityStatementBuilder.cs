using System;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;

namespace Spark.Engine.Service.FhirServiceExtensions
{
    public static class CapabilityStatementBuilder
    {


        public static CapabilityStatement CreateServer(string server, string publisher,
            string fhirVersion)
        {
            var capabilityStatement = new CapabilityStatement
            {
                Name = server,
                Publisher = publisher,
                //Version = serverVersion,
                FhirVersion = fhirVersion,
                AcceptUnknown = CapabilityStatement.UnknownContentCode.No,
                Date = Date.Today().Value
            };
          //  capabilityStatement.AddServer();
            return capabilityStatement;
        }

        public static CapabilityStatement.RestComponent AddRestComponent(this CapabilityStatement capabilityStatement,
            bool isServer, string documentation = null)
        {
            var server = new CapabilityStatement.RestComponent()
            {
                Mode =
                    isServer
                        ? CapabilityStatement.RestfulCapabilityMode.Server
                        : CapabilityStatement.RestfulCapabilityMode.Client
            };

            if (documentation != null)
            {
                server.Documentation = documentation;
            }

            capabilityStatement.Rest.Add(server);
            return server;
        }

        public static CapabilityStatement AddServer(this CapabilityStatement capabilityStatement)
        {
            capabilityStatement.AddRestComponent(true);
            return capabilityStatement;
        }

        public static CapabilityStatement.RestComponent Server(this CapabilityStatement capabilityStatement)
        {
            var server =
                capabilityStatement.Rest.FirstOrDefault(r => r.Mode == CapabilityStatement.RestfulCapabilityMode.Server);
            return server ?? capabilityStatement.AddRestComponent(true);
        }

        public static CapabilityStatement AddAllCoreResources(this CapabilityStatement capabilityStatement, bool readhistory, bool updatecreate, CapabilityStatement.ResourceVersionPolicy versioning)
        {
            foreach (var resource in ModelInfo.SupportedResources)
            {
                capabilityStatement.AddSingleResourceComponent(resource, readhistory, updatecreate, versioning);
            }
            return capabilityStatement;
        }

        public static CapabilityStatement AddSingleResourceComponent(this CapabilityStatement capabilityStatement, string resourcetype, bool readhistory, bool updatecreate, CapabilityStatement.ResourceVersionPolicy versioning, ResourceReference profile = null)
        {
            var resource = new CapabilityStatement.ResourceComponent
            {
                Type = (ResourceType)Enum.Parse(typeof(ResourceType), resourcetype, true),
                Profile = profile,
                ReadHistory = readhistory,
                UpdateCreate = updatecreate,
                Versioning = versioning
            };

            capabilityStatement.Server().Resource.Add(resource);
            return capabilityStatement;
        }

        public static CapabilityStatement.RestComponent Rest(this CapabilityStatement conformance)
        {
            return conformance.Rest.FirstOrDefault();
        }

        public static CapabilityStatement AddSummaryForAllResources(this CapabilityStatement capabilityStatement)
        {
            var firstOrDefault = capabilityStatement.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var resource in firstOrDefault.Resource.ToList())
                {
                    var p = new CapabilityStatement.SearchParamComponent
                    {
                        Name = "_summary",
                        Type = SearchParamType.String,
                        Documentation = "Summary for resource"
                    };
                    resource.SearchParam.Add(p);
                }
            return capabilityStatement;
        }

        public static CapabilityStatement AddCoreSearchParamsAllResources(this CapabilityStatement capabilityStatement)
        {
            var firstOrDefault = capabilityStatement.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var r in firstOrDefault.Resource.ToList())
                {
                    capabilityStatement.Rest().Resource.Remove(r);
                    capabilityStatement.Rest().Resource.Add(AddCoreSearchParamsResource(r));
                }
            return capabilityStatement;
        }


        public static CapabilityStatement AddSearchSetInteraction(this CapabilityStatement conformance)
        {
            var searchSet = CapabilityStatement.SystemRestfulInteraction.SearchSystem;
            conformance.AddSystemInteraction(searchSet);

            return conformance;
        }

        public static CapabilityStatement.ResourceComponent AddCoreSearchParamsResource(CapabilityStatement.ResourceComponent resourcecomp)
        {
            var parameters = ModelInfo.SearchParameters.Where(sp => sp.Resource == resourcecomp.Type.GetLiteral())
                            .Select(sp => new CapabilityStatement.SearchParamComponent
                            {
                                Name = sp.Name,
                                Type = sp.Type,
                                Documentation = sp.Description,

                            });

            resourcecomp.SearchParam.AddRange(parameters);
            return resourcecomp;
        }

        public static CapabilityStatement AddSearchTypeInteractionForResources(this CapabilityStatement conformance)
        {
            var firstOrDefault = conformance.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var r in firstOrDefault.Resource.ToList())
                {
                    conformance.Rest().Resource.Remove(r);
                    conformance.Rest().Resource.Add(AddSearchType(r));
                }
            return conformance;
        }

        public static CapabilityStatement.ResourceComponent AddSearchType(CapabilityStatement.ResourceComponent resourcecomp)
        {
            var type = CapabilityStatement.TypeRestfulInteraction.SearchType;
            var interaction = AddSingleResourceInteraction(resourcecomp, type);
            resourcecomp.Interaction.Add(interaction);
            return resourcecomp;
        }


        public static CapabilityStatement AddAllInteractionsForAllResources(this CapabilityStatement conformance)
        {
            var firstOrDefault = conformance.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var r in firstOrDefault.Resource.ToList())
                {
                    conformance.Rest().Resource.Remove(r);
                    conformance.Rest().Resource.Add(AddAllResourceInteractions(r));
                }
            return conformance;
        }

        public static CapabilityStatement.ResourceComponent AddAllResourceInteractions(CapabilityStatement.ResourceComponent resourcecomp)
        {
            foreach (CapabilityStatement.TypeRestfulInteraction type in Enum.GetValues(typeof(CapabilityStatement.TypeRestfulInteraction)))
            {
                var interaction = AddSingleResourceInteraction(resourcecomp, type);
                resourcecomp.Interaction.Add(interaction);
            }
            return resourcecomp;
        }

        public static CapabilityStatement.ResourceInteractionComponent AddSingleResourceInteraction(CapabilityStatement.ResourceComponent resourcecomp, CapabilityStatement.TypeRestfulInteraction type)
        {
            var interaction = new CapabilityStatement.ResourceInteractionComponent { Code = type };
            return interaction;

        }

        public static CapabilityStatement AddAllSystemInteractions(this CapabilityStatement conformance)
        {
            foreach (CapabilityStatement.SystemRestfulInteraction code in Enum.GetValues(typeof(CapabilityStatement.SystemRestfulInteraction)))
            {
                conformance.AddSystemInteraction(code);
            }
            return conformance;
        }

        public static void AddSystemInteraction(this CapabilityStatement conformance, CapabilityStatement.SystemRestfulInteraction code)
        {
            var interaction = new CapabilityStatement.SystemInteractionComponent { Code = code };

            conformance.Rest().Interaction.Add(interaction);
        }

        public static void AddOperation(this CapabilityStatement conformance, string name, ResourceReference definition)
        {
            var operation = new CapabilityStatement.OperationComponent
            {
                Name = name,
                Definition = definition
            };

            conformance.Server().Operation.Add(operation);
        }

        public static string ConformanceToXML(this CapabilityStatement conformance)
        {
            return FhirSerializer.SerializeResourceToXml(conformance);
        }

    }
}
