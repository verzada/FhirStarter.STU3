using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace FhirStarter.Bonfire.STU3.Interface
{
    public interface IFhirStructureDefinitionService
    {
        ICollection<StructureDefinition> GetStructureDefinitions();
    }
}
