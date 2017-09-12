using FhirStarter.Inferno.Services;
using Hl7.Fhir.Model;
using NUnit.Framework;
using Spark.Engine.Core;

namespace FhirStarter.UnitTests.Services
{
    [TestFixture]
    internal class ExamplePatientServiceUnitTest
    {

        private ExamplePatientService _patientService;

        [SetUp]
        public void Setup()
        {
            _patientService = new ExamplePatientService();
        }

        
        [Test]
        public void TestGetRest()
        {
            var value = _patientService.GetRestDefinition();
            Assert.IsNotNull(value);
        }

        [Test]
        public void TestCreatePatient()
        {
            var patient = new Patient();
            var result = _patientService.Create(new Key("", "", "", ""), patient);
        }
    }
}
