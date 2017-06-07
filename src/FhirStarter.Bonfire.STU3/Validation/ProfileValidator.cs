using Hl7.Fhir.Validation;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;

namespace FhirStarter.Bonfire.STU3.Validation
{
    public class ProfileValidator
    {
        private readonly object _locked = "";
        //private const string ProfileLocation = @"\Profile";
        private static Validator _validator;
        public ProfileValidator(bool validateXsd, bool showTrace, bool reloadValidator, string profileFolder)
        {
            if (_validator != null && !reloadValidator) return;
            var coreSource = new CachedResolver(ZipSource.CreateValidationSource());
            var cachedResolver = new CachedResolver(new DirectorySource(profileFolder, includeSubdirectories: true));
            var combinedSource = new MultiResolver(cachedResolver, coreSource);
            var settings = new ValidationSettings
            {
                EnableXsdValidation = validateXsd,
                GenerateSnapshot = true,
                Trace = showTrace,
                ResourceResolver = combinedSource,
                ResolveExteralReferences = true,SkipConstraintValidation = false
            };
            _validator = new Validator(settings);
        }

        public OperationOutcome Validate(XmlReader reader, bool onlyErrors)
        {
            //lock (_locked)
            //{
                var result = _validator.Validate(reader);
                if (!onlyErrors)
                {
                    return result;
                }
                var invalidItems = (from item in result.Issue
                    let error = item.Severity != null && item.Severity.Value == OperationOutcome.IssueSeverity.Error
                    where error
                    select item).ToList();

                result.Issue = invalidItems;
                return result;
            //}


        }
    }
}
