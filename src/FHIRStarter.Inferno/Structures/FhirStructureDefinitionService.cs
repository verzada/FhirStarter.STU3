using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Inferno.Structures
{
    public class FhirStructureDefinitionService:IFhirStructureDefinitionService
    {
        
        public ICollection<StructureDefinition> GetStructureDefinitions()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + @"bin\Resources\StructureDefinitions";
            var structureDefinitionFiles = Directory.GetFiles(directory);
            return structureDefinitionFiles.Select(File.ReadAllText).Select(content => new FhirXmlParser().Parse<StructureDefinition>(content)).ToList();
            

        }
    }
}