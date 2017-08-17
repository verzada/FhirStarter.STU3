using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Linq;
using FhirStarter.Bonfire.STU3.Filter;
using FhirStarter.Bonfire.STU3.Interface;
using FhirStarter.Bonfire.STU3.Service;
using FhirStarter.Bonfire.STU3.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Infrastructure;

namespace FhirStarter.Flare.STU3.Controllers
{
    [RoutePrefix("fhir"), EnableCors("*", "*", "*", "*")]
    [RouteDataValuesOnly]
    [ExceptionFilter]
    public class FhirController : ApiController
    {
        private readonly ICollection<IFhirService> _fhirServices;
        private readonly IFhirStructureDefinitionService _fhirStructureDefinitionService;
        private readonly ServiceHandler _handler = new ServiceHandler();
        private readonly ProfileValidator _profileValidator;

        public FhirController(ICollection<IFhirService> services, IFhirStructureDefinitionService fhirStructureDefinitionService, ProfileValidator profileValidator)
        {
            _fhirServices = services;
            _profileValidator = profileValidator;
        }

        public FhirController(ICollection<IFhirService> services, IFhirStructureDefinitionService fhirStructureDefinitionService)
        {
            _fhirServices = services;
            _fhirStructureDefinitionService = fhirStructureDefinitionService;
        }

        [HttpGet, Route("StructureDefinition/{id}")]
        public HttpResponseMessage GetStructureDefinition(string id)
        {
            var structureDefinitionNames = _handler.GetStructureDefinitionNames(_fhirServices);
            if (!structureDefinitionNames.Contains(id))
            {
                throw new FhirOperationException($"{nameof(StructureDefinition)} {id} not found", HttpStatusCode.InternalServerError);
            }
            var structureDefinitions = _fhirStructureDefinitionService.GetStructureDefinitions();
            var structureDefinition = structureDefinitions.FirstOrDefault(definition => definition.Name.Equals(id));
            if (structureDefinition != null)
            {
                return SendResponse(structureDefinition);
            }
            throw new FhirOperationException($"{nameof(StructureDefinition)} for {id} not found", HttpStatusCode.InternalServerError);
            
        }

        

        [HttpGet, Route("{type}/{id}"), Route("{type}/identifier/{id}")]
        public HttpResponseMessage Read(string type, string id)
        {
            if (type.Equals(nameof(OperationDefinition)))
            {
                var operationDefinitions = _handler.GetOperationDefinitions(id, _fhirServices);
                return SendResponse(operationDefinitions);
            }

            var service = _handler.FindServiceFromList(_fhirServices, type);
            var result = service.Read(id);

            return SendResponse(result);
        }

       
        [HttpGet, Route("{type}")]
        public HttpResponseMessage Read(string type)
        {
            var service = _handler.FindServiceFromList(_fhirServices, type);
            var parameters = Request.GetSearchParams();
            if (!(parameters.Parameters.Count > 0)) return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
            var results = service.Read(parameters);
            return SendResponse(results);
        }

        [HttpGet, Route("")]
        // ReSharper disable once InconsistentNaming
        public HttpResponseMessage Query(string _query)
        {
            var searchParams = Request.GetSearchParams();
            var service = _handler.FindServiceFromList(_fhirServices, searchParams.Query);
            var result = service.Read(searchParams);

            return SendResponse(result);
        }

        [HttpPost, Route("{type}")]
        public HttpResponseMessage Create(string type, Resource resource)
        {
            var service = _handler.FindServiceFromList(_fhirServices, type);
            return _handler.ResourceCreate(type, resource, service);
        }

        [HttpPut, Route("{type}/{id}")]
        public HttpResponseMessage Update(string type, string id, Resource resource)
        {
            var service = _handler.FindServiceFromList(_fhirServices, type);
            return _handler.ResourceUpdate(type, id, resource, service);
        }

        [HttpDelete, Route("{type}/{id}")]
        public HttpResponseMessage Delete(string type, string id)
        {
            var service = _handler.FindServiceFromList(_fhirServices, type);
            return _handler.ResourceDelete(type, Key.Create(type, id), service);
        }

        [HttpPatch, Route("{type}/{id}")]
        public HttpResponseMessage Patch(string type, string id, Resource resource)
        {
            var service = _handler.FindServiceFromList(_fhirServices, type);
            return _handler.ResourcePatch(type, Key.Create(type, id), resource, service);
        }

        private HttpResponseMessage SendResponse(Base resource)
        {
           
            var headers = Request.Headers;
            var accept = headers.Accept;
            var returnJson = ReturnJson(accept);

            resource = ValidateResource(resource);

            StringContent httpContent;
            if (!returnJson)
            {
                var xml = FhirSerializer.SerializeToXml(resource);
                httpContent =
                    new StringContent(xml, Encoding.UTF8,
                     FhirMediaType.XmlResource);
            }
            else
            {
                httpContent =
                    new StringContent(FhirSerializer.SerializeToJson(resource), Encoding.UTF8,
                     FhirMediaType.JsonResource);
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = httpContent };
            return response;
        }

        private Base ValidateResource(Base resource)
        {
            if (_profileValidator == null) return resource;
            var resourceAsXDocument = XDocument.Parse(FhirSerializer.SerializeToXml(resource));
            var validationResult = _profileValidator.Validate(resourceAsXDocument.CreateReader(), true);
            if (validationResult.Issue.Count > 0)
            {
                resource = validationResult;
            }
            return resource;
        }

        private static bool ReturnJson(HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> accept)
        {
            var jsonHeaders = Hl7.Fhir.Rest.ContentType.JSON_CONTENT_HEADERS;
            var returnJson = false;
            foreach (var x in accept)
            {
                foreach (var y in jsonHeaders)
                {
                    if (!x.MediaType.Contains(y)) continue;
                    returnJson = true;
                    break;
                }
            }
            return returnJson;
        }

        [HttpGet, Route("metadata")]
        public HttpResponseMessage MetaData()
        {
            var headers = Request.Headers;
            var accept = headers.Accept;
            var returnJson = accept.Any(x => x.MediaType.Contains(FhirMediaType.HeaderTypeJson));

            StringContent httpContent;
            var metaData = _handler.CreateMetadata(_fhirServices);
            if (!returnJson)
            {
                var xml = FhirSerializer.SerializeToXml(metaData);
                httpContent =
                    new StringContent(xml, Encoding.UTF8,
                        "application/xml");
            }
            else
            {
                httpContent =
                    new StringContent(FhirSerializer.SerializeToJson(metaData), Encoding.UTF8,
                        "application/json");
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = httpContent };
            return response;
        }
    }
}
