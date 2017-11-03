using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using FhirStarter.Bonfire.STU3.Interface;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;

namespace FhirStarter.Inferno.Template.Structures
{
    public class FhirStructureDefinitionService : AbstractStructureDefinitionService
    {
        private readonly string _structureDefinitionsFolder = AppDomain.CurrentDomain.BaseDirectory + @"bin\Resources\StructureDefinitions";

        public override ICollection<StructureDefinition> GetStructureDefinitions()
        {
            var structureDefinitionFiles = Directory.GetFiles(_structureDefinitionsFolder);
            return structureDefinitionFiles.Select(File.ReadAllText).Select(content => new FhirXmlParser().Parse<StructureDefinition>(content)).ToList();
        }

        protected override IResourceResolver GetResourceResolver()
        {
            var cachedResolver = new CachedResolver(new DirectorySource(_structureDefinitionsFolder, true));
            return cachedResolver;
        }

        protected override bool IsValidationOn()
        {
            var enableValidation = ConfigurationManager.AppSettings["EnableValidation"];
            return enableValidation != null && Convert.ToBoolean(enableValidation);
        }
    }
}