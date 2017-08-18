using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;

namespace FhirStarter.Bonfire.STU3.Interface
{
    public interface IFhirStructureDefinitionService
    {
        ICollection<StructureDefinition> GetStructureDefinitions();        
        Validator GetValidator();
    }
}
