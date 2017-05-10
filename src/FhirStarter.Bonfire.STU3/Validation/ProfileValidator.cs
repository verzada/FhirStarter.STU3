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
        private const string ProfileLocation = @"\Profile";
        private static Validator _validator;
        public ProfileValidator(bool validateXsd, bool showTrace, bool reloadValidator, Assembly profileAssembly)
        {
            if (_validator == null || reloadValidator)
            {
                var location = new Uri(profileAssembly.GetName().CodeBase);
                var directoryInfo = new FileInfo(location.AbsolutePath).Directory;
                if (directoryInfo != null)
                {
                    var coreSource = new CachedResolver(ZipSource.CreateValidationSource());
                    var profilePath = Path.Combine(directoryInfo.FullName) + ProfileLocation;
                    var cachedResolver = new CachedResolver(new DirectorySource(profilePath, includeSubdirectories: true));
                    var combinedSource = new MultiResolver(cachedResolver, coreSource);
                    var settings = new Hl7.Fhir.Validation.ValidationSettings
                    {
                        EnableXsdValidation = validateXsd,
                        GenerateSnapshot = true,
                        Trace = showTrace,
                        ResourceResolver = combinedSource
                    };
                    _validator = new Validator(settings);
                }
                else
                {
                    throw new IOException("Cannot retrieve directoryinfo");
                }
            }


        }

        public OperationOutcome Validate(XmlReader reader, bool onlyErrors)
        {
            lock (_locked)
            {
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
            }


        }
    }
}
