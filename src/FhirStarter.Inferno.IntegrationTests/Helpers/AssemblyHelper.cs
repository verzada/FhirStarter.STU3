using System.IO;
using System.Linq;
using System.Reflection;

namespace FhirStarter.Inferno.IntegrationTests.Helpers
{
    public static class AssemblyHelper
    {
        public static Stream GetStream(string resourceName, Assembly assembly)
        {
            using (var enumerator = assembly.GetManifestResourceNames().Where(name => name.EndsWith(resourceName)).GetEnumerator())
            {
                if (!enumerator.MoveNext()) throw new IOException("Cannot find resource: " + resourceName);
                var current = enumerator.Current;
                return assembly.GetManifestResourceStream(current);
            }            
        }

        public static Assembly GetResourceAssembly(string name)
        {
            return Assembly.Load(name);
        }
    }
}
