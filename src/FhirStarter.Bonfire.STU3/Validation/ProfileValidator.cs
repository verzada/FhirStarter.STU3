using System;
using System.Collections.Generic;
using Hl7.Fhir.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Bonfire.STU3.Validation
{
    public class ProfileValidator
    {
        private static Validator _validator;
        public ProfileValidator(Validator validator)
        {
            if (_validator == null)
            {
                _validator = validator;
            };            
        }

        public OperationOutcome Validate(Resource resource, bool onlyErrors=true, bool threadedValidation=true)
        {
            
            if (!(resource is Bundle) || !threadedValidation)
            {
                using (var reader = XDocument.Parse(FhirSerializer.SerializeResourceToXml(resource)).CreateReader())
                {
                    return RunValidation(onlyErrors, reader);
                }
            }
            var bundle = (Bundle)resource;
            return RunBundleValidation(onlyErrors, bundle);
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
            foreach (var item in serialItems)
            {
                var localOperationOutCome = RunValidation(onlyErrors,
                    XDocument.Parse(FhirSerializer.SerializeResourceToXml(item)).CreateReader());
                operationOutcome.Issue.AddRange(localOperationOutCome.Issue);
            }

            Parallel.ForEach(parallellItems, new ParallelOptions {MaxDegreeOfParallelism = parallellItems.Count},
                loopedResource =>
                {
                    using (var reader = XDocument.Parse(FhirSerializer.SerializeResourceToXml(loopedResource)).CreateReader())
                    {
                        var localOperationOutCome = RunValidation(onlyErrors, reader);

                        operationOutcome.Issue.AddRange(localOperationOutCome.Issue);
                    }
                });

            //TODO: Validering av selve bundlen
            return operationOutcome;
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
