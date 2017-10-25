using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace FhirStarter.Flare.STU3.Helper
{
    public static class AssemblyLoaderHelper
    {
        public static ICollection<Assembly> GetFhirServiceAssemblies()
        {
            var assemblyNames = ConfigurationManager.AppSettings["FhirServiceAssemblies"];
            var assemblyNamesSplit = assemblyNames.Split(',');
            return assemblyNamesSplit.Select(assemblyName => Assembly.Load(assemblyName.Trim())).ToList();
        }
    }
}
