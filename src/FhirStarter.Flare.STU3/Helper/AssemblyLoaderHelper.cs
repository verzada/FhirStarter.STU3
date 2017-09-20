using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FhirStarter.Flare.STU3.Helper
{
    public static class AssemblyLoaderHelper
    {
        public static ICollection<Assembly> GetReflectionAssemblies()
        {
            var assemblyNames = ConfigurationManager.AppSettings["ReflectionAssemblies"];
            var assemblyNamesSplit = assemblyNames.Split(',');
            return assemblyNamesSplit.Select(assemblyName => Assembly.Load(assemblyName.Trim())).ToList();
        }
    }
}
