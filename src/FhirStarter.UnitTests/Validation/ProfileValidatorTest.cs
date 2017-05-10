using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FhirStarter.Bonfire.STU3.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NUnit.Framework;

namespace FhirStarter.UnitTests.Validation
{
    [TestFixture]
    internal class ProfileValidatorTest
    {
        private ProfileValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new ProfileValidator(true, true, false, Assembly.Load("FhirStarter.Inferno"));
        }

        [TestCase("ValidPatient.xml")]
        public void TestValidatePatientResponse(string xmlResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            Patient patient = null;
            var item = names.FirstOrDefault(t => t.EndsWith(xmlResource));
            XDocument xDocument = null;
            if (item != null)
            {

                using (var stream = assembly.GetManifestResourceStream(item))
                {
                    xDocument = XDocument.Load(stream);
                }
                patient = new FhirXmlParser().Parse<Patient>(xDocument.ToString());

            }
            Assert.IsNotNull(patient);

            var validResource = _validator.Validate(xDocument.CreateReader(), true);
            Console.WriteLine(FhirSerializer.SerializeToXml(validResource));
        }
    }
}
