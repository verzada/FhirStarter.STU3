using System;
using System.Collections.Generic;
using System.IO;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;

namespace FhirStarter.Bonfire.STU3.Interface
{
    public abstract class AbstractStructureDefinitionService
    {
        public abstract ICollection<StructureDefinition> GetStructureDefinitions();
        protected abstract IResourceResolver GetResourceResolver();

        protected abstract bool IsValidationOn(); 

        public Validator GetValidator()
        {
            if (!IsValidationOn())
            {
                return null;
            }
            
            var zipSource = ZipSource.CreateValidationSource();            
            var coreSource = new CachedResolver(zipSource);
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
    }
}
