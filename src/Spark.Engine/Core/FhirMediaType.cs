/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Net.Http.Headers;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

//using System.Web.Http;

namespace Spark.Engine.Core
{

    public static class FhirMediaType
    {
        // cannot use +fhir - experiencing crashes when using it in the browser
        // can be changed lateron when it's necessary
        public const string XmlResource = "application/xml";
        public const string JsonResource = "application/json";
        public const string BinaryResource = "application/fhir";
        public const string HeaderTypeJson = "json";
        public const string HeaderTypeXml = "xml";

        private static string GetContentType(Type type, ResourceFormat format)
        {
            if (typeof(Resource).IsAssignableFrom(type) || type == typeof(Resource))
            {
                switch (format)
                {
                    case ResourceFormat.Json: return JsonResource;
                    case ResourceFormat.Xml: return XmlResource;
                    default: return XmlResource;
                }
            }
            return "application/octet-stream";
        }

        public static MediaTypeHeaderValue GetMediaTypeHeaderValue(Type type, ResourceFormat format)
        {
            var mediatype = GetContentType(type, format);
            var header = new MediaTypeHeaderValue(mediatype) {CharSet = Encoding.UTF8.WebName};
            return header;
        }
    }

   

}