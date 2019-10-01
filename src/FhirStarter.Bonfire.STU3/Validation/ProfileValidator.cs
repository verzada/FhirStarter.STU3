using System.Collections.Generic;
using Hl7.Fhir.Validation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Bonfire.STU3.Validation
{
    public class ProfileValidator
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Validator _validator;
        private static bool _addResourceResultToIssue;

        public ProfileValidator(Validator validator, bool addResourceResultToIssue)
        {
            if (_validator == null)
            {
                _validator = validator;
            };
            _addResourceResultToIssue = addResourceResultToIssue;
        }

        public OperationOutcome Validate(Resource resource, bool onlyErrors=true, bool threadedValidation=true)
        {
            OperationOutcome validationError;
            if (resource.ResourceType != ResourceType.Bundle || !threadedValidation)
            {
                var xmlSerializer = new FhirXmlSerializer();
                //    using (var reader = XDocument.Parse(FhirSerializer.SerializeResourceToXml(resource)).CreateReader())
                using (var reader = XDocument.Parse(xmlSerializer.SerializeToString(resource)).CreateReader())
                {
                    validationError =  RunValidation(onlyErrors, reader);
                }
            }
            else
            {
                var bundle = (Bundle)resource;
                validationError =  RunBundleValidation(onlyErrors, bundle);
            }

            if (validationError.Issue.Count > 0)
            {
                var serializer = new FhirXmlSerializer(new SerializerSettings{Pretty = true});

                var resourceString = serializer.SerializeToString(resource);
                var validationErrorSerializeToString = serializer.SerializeToString(validationError);

                if (_addResourceResultToIssue)
                {
                    var resourceIssue = new OperationOutcome.IssueComponent { Diagnostics = resourceString };
                    validationError.Issue.Add(resourceIssue);
                }

                Log.Warn("Validation failed");
                Log.Warn("Response: " + resourceString);
                Log.Warn("Response:" + validationErrorSerializeToString);                                
            }

            return validationError;
        }

        private static OperationOutcome RunBundleValidation(bool onlyErrors, Bundle bundle)
        {
            var operationOutcome = new OperationOutcome();

            var itemsRun = new List<string>();
            var serialItems = new List<Resource>();
            var parallellItems = new List<Resource>();
            foreach (var item in bundle.Entry)
            {
                if (itemsRun.Contains(item.Resource.TypeName))
                {
                    parallellItems.Add(item.Resource);
                }
                else
                {
                    serialItems.Add(item.Resource);
                    itemsRun.Add(item.Resource.TypeName);
                }
            }
            RunSerialValidation(onlyErrors, serialItems, operationOutcome);
            RunParallellValidation(onlyErrors, parallellItems, operationOutcome);
            //TODO: Validering av selve bundlen
            return operationOutcome;
        }

        private static void RunParallellValidation(bool onlyErrors, List<Resource> parallellItems, OperationOutcome operationOutcome)
        {
            var xmlSerializer = new FhirXmlSerializer();
            if (parallellItems.Count > 0)
            {
                Parallel.ForEach(parallellItems, new ParallelOptions {MaxDegreeOfParallelism = parallellItems.Count},
                    loopedResource =>
                    {
                      
                        //using (var reader = XDocument.Parse(FhirSerializer.SerializeResourceToXml(loopedResource))
                        using (var reader = XDocument.Parse(xmlSerializer.SerializeToString(loopedResource))
                   .CreateReader())
                        {
                            var localOperationOutCome = RunValidation(onlyErrors, reader);

                            operationOutcome.Issue.AddRange(localOperationOutCome.Issue);
                        }
                    });
            }
        }

        private static void RunSerialValidation(bool onlyErrors, List<Resource> serialItems, OperationOutcome operationOutcome)
        {
            var xmlSerializer = new FhirXmlSerializer();
            foreach (var item in serialItems)
            {
                var localOperationOutCome = RunValidation(onlyErrors,
                //   XDocument.Parse(FhirSerializer.SerializeResourceToXml(item)).CreateReader());
                    XDocument.Parse(xmlSerializer.SerializeToString(item)).CreateReader());
                operationOutcome.Issue.AddRange(localOperationOutCome.Issue);
            }
        }

        private static OperationOutcome RunValidation(bool onlyErrors, XmlReader reader)
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
