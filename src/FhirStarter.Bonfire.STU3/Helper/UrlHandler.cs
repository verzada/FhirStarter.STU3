using System;
using System.Web;
using Hl7.Fhir.Model;

namespace FhirStarter.Bonfire.STU3.Helper
{
    public static class UrlHandler
    {
        public static string GetUrlForOperationDefinition(HttpContext context, string fhirUrlParameter, string alias)
        {
            return GetParentUrl(context, fhirUrlParameter) + fhirUrlParameter + nameof(OperationDefinition) + "/" + alias;
        }

        private static string GetParentUrl(HttpContext context, string fhirUrlParameter)
        {
            var originalPath = GetContextRequestUrl(context);

            if (originalPath.Contains(fhirUrlParameter))
            {
                var subString = originalPath.Substring(0,
                    GetContextRequestUrl(context).IndexOf(fhirUrlParameter, 0, StringComparison.Ordinal));
                return subString;
            }
            return originalPath;
        }

        private static string GetContextRequestUrl(HttpContext context)
        {
            var currentUrl = context.Request.Url;
            var path = currentUrl.AbsoluteUri;

            var uri = new Uri(path);

            var originalString = uri.OriginalString;
            return originalString;
        }
    }
}
