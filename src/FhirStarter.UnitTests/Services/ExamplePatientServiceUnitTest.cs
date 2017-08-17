using FhirStarter.Inferno.Services;
using NUnit.Framework;

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
    }
}
