using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var assembly = Assembly.GetExecutingAssembly();
            var location = new Uri(assembly.GetName().CodeBase);
            var directoryInfo = new FileInfo(location.AbsolutePath).Directory;
            Debug.Assert(directoryInfo != null, "directoryInfo != null");
            Debug.Assert(directoryInfo.FullName != null, "directoryInfo.FullName != null");
            _validator = new ProfileValidator(true, true, false, directoryInfo.FullName + @"\Resources\StructureDefinitions");
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

        [TestCase("DiagnosticReport.xml")]
        public void TestValidateDiagnosticReport(string xmlResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            DiagnosticReport diagnosticReport = null;
            var item = names.FirstOrDefault(t => t.EndsWith(xmlResource));
            XDocument xDocument = null;
            if (item != null)
            {

                using (var stream = assembly.GetManifestResourceStream(item))
                {
                    xDocument = XDocument.Load(stream);
                }
                diagnosticReport = new FhirXmlParser().Parse<DiagnosticReport>(xDocument.ToString());

            }
            Assert.IsNotNull(diagnosticReport);

            var validResource = _validator.Validate(xDocument.CreateReader(), true);
            Console.WriteLine(FhirSerializer.SerializeToXml(validResource));
        }

        [TestCase("MedicationStatement.xml")]
        public void TestValidateMedicationStatement(string xmlResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            MedicationStatement medicationStatement = null;
            var item = names.FirstOrDefault(t => t.EndsWith(xmlResource));
            XDocument xDocument = null;
            if (item != null)
            {

                using (var stream = assembly.GetManifestResourceStream(item))
                {
                    xDocument = XDocument.Load(stream);
                }
                medicationStatement = new FhirXmlParser().Parse<MedicationStatement>(xDocument.ToString());

            }
            Assert.IsNotNull(medicationStatement);

            var validResource = _validator.Validate(xDocument.CreateReader(), true);
            Console.WriteLine(FhirSerializer.SerializeToXml(validResource));
        }

        [TestCase("BundleWithMedicationStatement.xml")]
        public void TestValidateBundleMedicationStatement(string xmlResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            Bundle bundle = null;
            var item = names.FirstOrDefault(t => t.EndsWith(xmlResource));
            XDocument xDocument = null;
            if (item != null)
            {

                using (var stream = assembly.GetManifestResourceStream(item))
                {
                    xDocument = XDocument.Load(stream);
                }
                bundle = new FhirXmlParser().Parse<Bundle>(xDocument.ToString());

            }
            Assert.IsNotNull(bundle);

            var validResource = _validator.Validate(xDocument.CreateReader(), true);
            Console.WriteLine(XDocument.Parse(FhirSerializer.SerializeToXml(validResource)).ToString());
            Assert.AreEqual(0, validResource.Issue.Count, "Should not get an operation outcome");
        }
    }
}
