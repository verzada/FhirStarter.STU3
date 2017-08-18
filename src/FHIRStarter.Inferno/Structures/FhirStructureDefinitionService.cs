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

namespace FhirStarter.Inferno.Structures
{
    public class FhirStructureDefinitionService:IFhirStructureDefinitionService
    {
        private readonly string _structureDefinitionsFolder = AppDomain.CurrentDomain.BaseDirectory + @"bin\Resources\StructureDefinitions";

        public ICollection<StructureDefinition> GetStructureDefinitions()
        {
            
            var structureDefinitionFiles = Directory.GetFiles(_structureDefinitionsFolder);
            return structureDefinitionFiles.Select(File.ReadAllText).Select(content => new FhirXmlParser().Parse<StructureDefinition>(content)).ToList();
            

        }

        public Validator GetValidator()
        {
            var enableValidation = ConfigurationManager.AppSettings["EnableValidation"];
            if (enableValidation == null || !Convert.ToBoolean(enableValidation)) return null;
            var coreSource = new CachedResolver(ZipSource.CreateValidationSource());
            var combinedSource = new MultiResolver(GetResourceResolver(), coreSource);
            var settings = new ValidationSettings
            {
                EnableXsdValidation = true,
                GenerateSnapshot = true,
                Trace = true,
                ResourceResolver = combinedSource,
                ResolveExteralReferences = true,
                SkipConstraintValidation = false
            };
            return new Validator(settings);
        }

        public IResourceResolver GetResourceResolver()
        {
            var cachedResolver = new CachedResolver(new DirectorySource(_structureDefinitionsFolder, true));
            return cachedResolver;
        }
    }
}