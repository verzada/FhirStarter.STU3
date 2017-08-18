using Hl7.Fhir.Validation;
using System.Linq;
using System.Xml;
using Hl7.Fhir.Model;

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

        public OperationOutcome Validate(XmlReader reader, bool onlyErrors)
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
