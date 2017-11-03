using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Configuration;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Spark.Engine.Core;
using Spark.Engine.Service.FhirServiceExtensions;

namespace FhirStarter.Bonfire.STU3.Service
{
   public class ServiceHandler
   {
       public static string MockupEnabled = nameof(MockupEnabled);
       private static readonly log4net.ILog Log =
           log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Base GetOperationDefinitions(string id, ICollection<IFhirService> services)
        {
            var service = services.FirstOrDefault(s => s.GetServiceResourceReference().Equals(id));
            if (service?.GetOperationDefinition() == null) return new OperationDefinition();

            var operationDefinitions = service.GetOperationDefinition();
            return operationDefinitions;
        }

        public HttpResponseMessage ResourceCreate(string type, Resource resource, IFhirBaseService service)
        {
            if (service != null && !string.IsNullOrEmpty(type) && resource != null)
            {
                var key = Key.Create(type);
                var result = service.Create(key, resource);
                if (result != null)
                    return result;
            }

            return new HttpResponseMessage(HttpStatusCode.Ambiguous);
        }

        public ICollection<string> GetStructureDefinitionNames(ICollection<IFhirService> services)
        {
            var list = new List<string>();
            foreach (var service in services)
            {
                foreach (var name in service.GetStructureDefinitionNames())
                {
                    if (!list.Contains(name))
                    {
                        list.Add(name);
                    }
                }
            }
            return list;
        }
        public HttpResponseMessage ResourceUpdate(string type, string id, Resource resource, IFhirBaseService service)
        {
            if (service == null || string.IsNullOrEmpty(type) || resource == null || string.IsNullOrEmpty(id))
                throw new ArgumentException("Service is null, cannot update resource of type " + type);
            var key = Key.Create(type, id);
            var result = service.Update(key, resource);
            if (result != null)
                return result;
            throw new ArgumentException("Service is null, cannot update resource of type " + type);
        }

        public HttpResponseMessage ResourceDelete(string type, Key key, IFhirBaseService service)
        {
            if (service != null)
            {
                return service.Delete(key);
            }
            throw new ArgumentException("Service is null, cannot update resource of type " + type);
        }

        public HttpResponseMessage ResourcePatch(string type, IKey key, Resource resource, IFhirBaseService service)
        {
            if (service != null)
            {
                return service.Patch(key, resource);
            }
            throw new ArgumentException("Service is null, cannot update resource of type " + type);
        }

        public IFhirBaseService FindServiceFromList(ICollection<IFhirService> services, ICollection<IFhirMockupService> mockupServices, string type)
        {
            if (IsMockupEnabled())
            {
                return GetMockupService(mockupServices, type);
            }

            if (GetService(services, type, out var fhirService)) return fhirService;
            throw new ArgumentException("There are no available services.");
        }

       private static bool GetService(ICollection<IFhirService> services, string type, out IFhirService fhirService)
       {
           fhirService = null;
           if (services != null && services.Any())
           {
               foreach (var service in services)
               {
                   if (service.GetServiceResourceReference().Equals(type))
                   {
                       {
                           fhirService = service;
                           return true;
                       }
                   }
               }
           }
           if (services != null && services.Count > 1)
           {
               throw new ArgumentException("The resource type " + type +
                                           " is not supported by the available services.");
           }
           return false;
       }

       private static IFhirMockupService GetMockupService(ICollection<IFhirMockupService> mockupServices, string type)
       {
           if (mockupServices != null && mockupServices.Any())
           {
               foreach (var mockup in mockupServices)
               {
                   if (mockup.GetServiceResourceReference().Equals(type))
                   {
                        Log.Info("EnableMockup is set to true, returning mockupservice of type " +type);
                       return mockup;
                   }
               }
           }
           throw new ArgumentException("Could not find any mockup services defined by the interface " +
                                       nameof(IFhirMockupService) +
                                       " despite having the EnabledMockup option in AppSettings in the web.config.");
       }

       public CapabilityStatement CreateMetadata(ICollection<IFhirService> services, AbstractStructureDefinitionService abstractStructureDefinitionService, string baseUrl)
        {
            if (!services.Any()) return new CapabilityStatement();
            var serviceName = MetaDataName(services);

            var fhirVersion = ModelInfo.Version;

            var fhirPublisher = GetFhirPublisher();
            var fhirDescription = GetFhirDescription();

            var conformance = CapabilityStatementBuilder.CreateServer(serviceName, fhirPublisher, fhirVersion);

            //conformance.AddUsedResources(services, false, false,
            //  CapabilityStatement.ResourceVersionPolicy.VersionedUpdate);

            //conformance.AddSearchSetInteraction().AddSearchTypeInteractionForResources();
            conformance.AddSearchTypeInteractionForResources();
            conformance = conformance.AddCoreSearchParamsAllResources(services);
            conformance = conformance.AddOperationDefintion(services);

            conformance.AcceptUnknown = CapabilityStatement.UnknownContentCode.Both;
            conformance.Experimental = true;
            conformance.Format = new[] { "xml", "json" };
            conformance.Description = new Markdown(fhirDescription);

            conformance.Profile = SetProfiles(abstractStructureDefinitionService);
                
            return conformance;
        }

       private static List<ResourceReference> SetProfiles(AbstractStructureDefinitionService abstractStructureDefinitionService)
       {
            var structureDefinitions = abstractStructureDefinitionService.GetStructureDefinitions();
            var profiles = structureDefinitions.Select(structureDefinition => new ResourceReference {Url = new Uri(structureDefinition.Url)}).ToList();
           return profiles;
       }

       private static string GetFhirDescription()
        {
            var fhirDescription = WebConfigurationManager.AppSettings["FhirDescription"];
            if (string.IsNullOrEmpty(fhirDescription))
            {
                fhirDescription = "Add FhirDescription key with the description of the service to appSettings in web.config.";
            }
            return fhirDescription;
        }

        private static string GetFhirPublisher()
        {
            var fhirPublisher = WebConfigurationManager.AppSettings["FhirPublisher"];
            if (string.IsNullOrEmpty(fhirPublisher))
            {
                fhirPublisher = "Add FhirPublisher key with the name of the publisher to appSettings in web.config.";
            }
            return fhirPublisher;
        }


        private string MetaDataName(ICollection<IFhirService> services)
        {
            var serviceName = services.Count > 1 ? "The following services are available: " : "The following service is available: ";

            var servicesAsArray = services.ToArray();
            for (var i = 0; i < services.Count; i++)
            {
                serviceName += servicesAsArray[i].GetServiceResourceReference();
                if (i < services.Count - 1)
                {
                    serviceName += " ";
                }
            }
            return serviceName;
        }

        public static bool IsMockupEnabled()
        {
            var stringValue = ConfigurationManager.AppSettings[MockupEnabled];
            var isMockupEnabled = Convert.ToBoolean(stringValue);
            return isMockupEnabled;
        }
     
    }
}

