using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FhirStarter.Bonfire.STU3.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using NUnit.Framework;

namespace FhirStarter.UnitTests.Validation
{
    [TestFixture]
    internal class ProfileValidatorTest
    {
        private ProfileValidator _profileValidator;

        [SetUp]
        public void Setup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var location = new Uri(assembly.GetName().CodeBase);
            var directoryInfo = new FileInfo(location.AbsolutePath).Directory;
            Debug.Assert(directoryInfo != null, "directoryInfo != null");
            Debug.Assert(directoryInfo.FullName != null, "directoryInfo.FullName != null");
            var cachedResolver = new CachedResolver(new DirectorySource(directoryInfo.FullName + @"\Resources\StructureDefinitions", includeSubdirectories: true));
            var coreSource = new CachedResolver(ZipSource.CreateValidationSource());            
            var combinedSource = new MultiResolver(cachedResolver, coreSource);
            var settings = new ValidationSettings
            {
                EnableXsdValidation = true,
                GenerateSnapshot = true,
                Trace = true,
                ResourceResolver = combinedSource,
                ResolveExteralReferences = true,
                SkipConstraintValidation = false
            };
            var validator = new Validator(settings);
            _profileValidator = new ProfileValidator(validator);


        }

        [TestCase("ValidPatient.xml")]
        [Ignore("Not in use")]
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

            var validResource = _profileValidator.Validate(patient, true, true);
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

            var validResource = _profileValidator.Validate(diagnosticReport, true, true);
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

            var validResource = _profileValidator.Validate(medicationStatement, true, true);
            Console.WriteLine(FhirSerializer.SerializeToXml(validResource));
        }

        //[TestCase("BundleWithMedicationStatement.xml")]
        [TestCase("EmptyBundleWithMedicationStatement.xml")]
        public void TestValidateBundleMedicationStatement(string xmlResource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            Bundle bundle = null;
            var item = names.FirstOrDefault(t => t.EndsWith(xmlResource));
            if (item != null)
            {
                XDocument xDocument;
                using (var stream = assembly.GetManifestResourceStream(item))
                {
                    xDocument = XDocument.Load(stream);
                }
                bundle = new FhirXmlParser().Parse<Bundle>(xDocument.ToString());

            }
            Assert.IsNotNull(bundle);

            var validResource = _profileValidator.Validate(bundle);
            Console.WriteLine(XDocument.Parse(FhirSerializer.SerializeToXml(validResource)).ToString());
            Assert.AreEqual(0, validResource.Issue.Count, "Should not get an operation outcome");
        }
    }
}
