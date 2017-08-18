using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Validation;

namespace FhirStarter.Bonfire.STU3.Interface
{
    public interface IFhirStructureDefinitionService
    {
        ICollection<StructureDefinition> GetStructureDefinitions();        
        Validator GetValidator();
    }
}
