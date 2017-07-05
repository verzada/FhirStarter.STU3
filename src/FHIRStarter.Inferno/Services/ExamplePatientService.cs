using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;

namespace FhirStarter.Inferno.Services
{
    public class ExamplePatientService : IFhirService
    {
        //Edit
        public ExamplePatientService()
        {
            int i = 0;
        }

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
            throw new InvalidDataException();
        }

        public static T CreateObjectFromXmlDocument<T>(XmlDocument source, string defaultNamespace = null)
        {
            T result;
            var xmlSerializer = new XmlSerializer(typeof(T), defaultNamespace);
            using (var xmlReader = XmlReader.Create(new StringReader(source.OuterXml)))
            {
                result = (T)xmlSerializer.Deserialize(xmlReader);
            }
            return result;
        }

       


        public List<string> GetSupportedResources()
        {
            return new List<string> {nameof(Patient)};
        }

     
        public OperationDefinition GetOperationDefinition()
        {
            return new OperationDefinition();
        }

       

        private static Base MockPatient()
        {
            var date = new FhirDateTime(DateTime.Now);

            return new Patient
            {
                Meta = new Meta { LastUpdated = date.ToDateTimeOffset(), Profile = new List<string> { "http://helse-nord.no/FHIR/profiles/Identification.Patient/Patient" } },
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
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Delete(IKey key)
        {
            throw new System.NotImplementedException();
        }

        public HttpResponseMessage Patch(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }
    }
}
