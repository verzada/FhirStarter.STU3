/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Spark.Engine.Service.FhirServiceExtensions
{

    public static class ConformanceBuilder
    {
        public static Conformance GetSparkConformance(string sparkVersion)
        {
            var vsn = ModelInfo.Version;
            var conformance = CreateServer("Spark", sparkVersion, "Furore", vsn);

            conformance.AddAllCoreResources(true, true, Conformance.ResourceVersionPolicy.VersionedUpdate);
            conformance.AddAllSystemInteractions().AddAllInteractionsForAllResources().AddCoreSearchParamsAllResources();
            conformance.AddSummaryForAllResources();
            conformance.AcceptUnknown = Conformance.UnknownContentCode.Both;
            conformance.Experimental = true;
            conformance.Format = new[] { "xml", "json" };
            conformance.Description = "This FHIR SERVER is a reference Implementation server built in C# on HL7.Fhir.Core (nuget) by Furore and others";

            return conformance;
        }

        public static Conformance CreateServer(string server, string serverVersion, string publisher, string fhirVersion)
        {
            var conformance = new Conformance
            {
                Name = server,
                Publisher = publisher,
                Version = serverVersion,
                FhirVersion = fhirVersion,
                AcceptUnknown = Conformance.UnknownContentCode.No,
                Date = Date.Today().Value
            };
            conformance.AddServer();
            return conformance;
           
        }

        public static Conformance.RestComponent AddRestComponent(this Conformance conformance, bool isServer, string documentation = null)
        {
            var server = new Conformance.RestComponent
            {
                Mode =
                    isServer ? Conformance.RestfulConformanceMode.Server : Conformance.RestfulConformanceMode.Client
            };

            if (documentation != null)
            {
                server.Documentation = documentation;
            }
            conformance.Rest.Add(server);
            return server;
        }

        public static Conformance AddServer(this Conformance conformance)
        {
            conformance.AddRestComponent(true);
            return conformance;
        }

        public static Conformance.RestComponent Server(this Conformance conformance)
        {
            var server = conformance.Rest.FirstOrDefault(r => r.Mode == Conformance.RestfulConformanceMode.Server);
            return server == null ? conformance.AddRestComponent(true) : server;
        }

        public static Conformance.RestComponent Rest(this Conformance conformance)
        {
            return conformance.Rest.FirstOrDefault();
        }

        public static Conformance AddAllCoreResources(this Conformance conformance, bool readhistory, bool updatecreate, Conformance.ResourceVersionPolicy versioning)
        {
            foreach (var resource in ModelInfo.SupportedResources)
            {
                conformance.AddSingleResourceComponent(resource, readhistory, updatecreate, versioning);
            }
            return conformance;
        }

        public static Conformance AddMultipleResourceComponents(this Conformance conformance, List<string> resourcetypes, bool readhistory, bool updatecreate, Conformance.ResourceVersionPolicy versioning)
        {
            foreach (var type in resourcetypes)
            {
                AddSingleResourceComponent(conformance, type, readhistory, updatecreate, versioning);
            }
            return conformance;
        }

        public static Conformance AddSingleResourceComponent(this Conformance conformance, string resourcetype, bool readhistory, bool updatecreate, Conformance.ResourceVersionPolicy versioning, ResourceReference profile = null)
        {
            var resource = new Conformance.ResourceComponent
            {
                Type = (ResourceType) Enum.Parse(typeof(ResourceType), resourcetype, true),
                Profile = profile,
                ReadHistory = readhistory,
                UpdateCreate = updatecreate,
                Versioning = versioning
            };

            conformance.Server().Resource.Add(resource);
            return conformance;
        }

        public static Conformance AddSummaryForAllResources(this Conformance conformance)
        {
            var firstOrDefault = conformance.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var resource in firstOrDefault.Resource.ToList())
                {
                    var p = new Conformance.SearchParamComponent
                    {
                        Name = "_summary",
                        Type = SearchParamType.String,
                        Documentation = "Summary for resource"
                    };
                    resource.SearchParam.Add(p);
                }
            return conformance;
        }

        public static Conformance AddCoreSearchParamsAllResources(this Conformance conformance)
        {
            var firstOrDefault = conformance.Rest.FirstOrDefault();
            if (firstOrDefault != null)
                foreach (var r in firstOrDefault.Resource.ToList())
                {
                    conformance.Rest().Resource.Remove(r);
                    conformance.Rest().Resource.Add(AddCoreSearchParamsResource(r));
                }
            return conformance;
        }


        public static Conformance.ResourceComponent AddCoreSearchParamsResource(Conformance.ResourceComponent resourcecomp)
        {
            var parameters = ModelInfo.SearchParameters.Where(sp => sp.Resource == resourcecomp.Type.GetLiteral())
                            .Select(sp => new Conformance.SearchParamComponent
                            {
                                Name = sp.Name,
                                Type = sp.Type,
                                Documentation = sp.Description,
                                
                            });

            resourcecomp.SearchParam.AddRange(parameters);
            return resourcecomp;
        }

        public static Conformance AddAllInteractionsForAllResources(this Conformance conformance)
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

        public static Conformance.ResourceComponent AddAllResourceInteractions(Conformance.ResourceComponent resourcecomp)
        {
            foreach (Conformance.TypeRestfulInteraction type in Enum.GetValues(typeof(Conformance.TypeRestfulInteraction)))
            {
                var interaction = AddSingleResourceInteraction(resourcecomp, type);
                resourcecomp.Interaction.Add(interaction);
            }
            return resourcecomp;
        }

        public static Conformance.ResourceInteractionComponent AddSingleResourceInteraction(Conformance.ResourceComponent resourcecomp, Conformance.TypeRestfulInteraction type)
        {
            var interaction = new Conformance.ResourceInteractionComponent {Code = type};
            return interaction;

        }

        public static Conformance AddAllSystemInteractions(this Conformance conformance)
        {
            foreach (Conformance.SystemRestfulInteraction code in Enum.GetValues(typeof(Conformance.SystemRestfulInteraction)))
            {
                conformance.AddSystemInteraction(code);
            }
            return conformance;
        }

        public static void AddSystemInteraction(this Conformance conformance, Conformance.SystemRestfulInteraction code)
        {
            var interaction = new Conformance.SystemInteractionComponent {Code = code};

            conformance.Rest().Interaction.Add(interaction);
        }

        public static void AddOperation(this Conformance conformance, string name, ResourceReference definition)
        {
            var operation = new Conformance.OperationComponent
            {
                Name = name,
                Definition = definition
            };

            conformance.Server().Operation.Add(operation);
        }

        public static string ConformanceToXML(this Conformance conformance)
        {
            return FhirSerializer.SerializeResourceToXml(conformance);
        }
    }
}

       