using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Xml.Linq;
using FhirStarter.Bonfire.STU3.Helper;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;

namespace FhirStarter.Inferno.Services
{
    public class ExamplePatientService : IFhirService
    {
        public string GetServiceResourceReference()
        {
            return nameof(Patient);
        }

        public CapabilityStatement.RestComponent GetRestDefinition()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (!name.EndsWith("ExampleServiceRest.xml")) continue;
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    var xDocument = XDocument.Load(stream);
                    var parser = new FhirXmlParser();
                    var item =
                        parser.Parse<CapabilityStatement>(xDocument.ToString());
                    return item.Rest[0];
                }
            }
            throw new InvalidDataException("Metadata information has not been added");
        }

        public OperationDefinition GetOperationDefinition()
        {
            var defintion = new OperationDefinition
            {
                Url = UrlHandler.GetUrlForOperationDefinition(HttpContext.Current, "fhir/", nameof(Patient)),
                Name = GetServiceResourceReference(),
                Status = PublicationStatus.Active,
                Kind = OperationDefinition.OperationKind.Query,
                Experimental = false,
                Code = GetServiceResourceReference(),
                Description = new Markdown("Search parameters for the test query service"),
                System = true,
                Instance = false,
                Parameter =
                    new List<OperationDefinition.ParameterComponent>
                    {
                        new OperationDefinition.ParameterComponent
                        {
                            Name = "Name",
                            Use = OperationParameterUse.In,
                            Type = FHIRAllTypes.String,
                            Min = 0,
                            Max = "1"
                        },
                        new OperationDefinition.ParameterComponent
                        {
                            Name = "Name:contains",
                            Use = OperationParameterUse.In,
                            Type = FHIRAllTypes.String,
                            Min = 0,
                            Max = "1"
                        },
                        new OperationDefinition.ParameterComponent
                        {
                            Name = "Name:exact",
                            Use = OperationParameterUse.In,
                            Type = FHIRAllTypes.String,
                            Min = 0,
                            Max = "1"
                        },
                        new OperationDefinition.ParameterComponent
                        {
                            Name = "Identifier",
                            Use = OperationParameterUse.In,
                            Type = FHIRAllTypes.String,
                            Min = 0,
                            Max = "1",
                            Documentation = "Query against the following: REKVIRENTKODE, HPNR or HER-ID"
                        },
                        new OperationDefinition.ParameterComponent
                        {
                            Name = "_lastupdated",
                            Use = OperationParameterUse.In,
                            Type = FHIRAllTypes.String,
                            Min = 0,
                            Max = "2",
                            Documentation =
                                "Equals" + " -- Note that the date format is yyyy-MM-ddTHH:mm:ss --"
                        }
                    }
            };
            return defintion;
        }

       

        private static Base MockPatient()
        {
            var date = new FhirDateTime(DateTime.Now);

            return new Patient
            {
                Meta = new Meta { LastUpdated = date.ToDateTimeOffset()},
                Id = "12345678901",
                Active = true,
                Name =
                    new List<HumanName>
                    {
                      new HumanName{Family = "Normann", Given = new List<string>{"Ola"}}
                    },
                Telecom =
                    new List<ContactPoint>
                    {
                        new ContactPoint {System = ContactPoint.ContactPointSystem.Phone, Value = "123467890"}
                    },
                Gender = AdministrativeGender.Male,
                BirthDate = "2000-01-01"
                
            };
        }

       

        public HttpResponseMessage Create(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public Base Read(SearchParams searchParams)
        {
            throw new ArgumentException("Using " + nameof(SearchParams) +
                                        " in Read(SearchParams searchParams) should throw an exception which is put into an OperationOutcomes issues");
        }

        public Base Read(string id)
        {
            return MockPatient();
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
    }
}
