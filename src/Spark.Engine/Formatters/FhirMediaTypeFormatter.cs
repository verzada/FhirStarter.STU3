/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Hl7.Fhir.Model;
using Spark.Engine.Core;
using Spark.Engine.Extensions;

namespace Spark.Engine.Formatters
{
    public abstract class FhirMediaTypeFormatter : MediaTypeFormatter
    {
        protected FhirMediaTypeFormatter()
        {
            SupportedEncodings.Clear();
            SupportedEncodings.Add(Encoding.UTF8);
        }

        private Entry _entry;
        protected HttpRequestMessage RequestMessage;

        private void SetEntryHeaders(HttpContentHeaders headers)
        {
            if (_entry == null) return;
            headers.LastModified = _entry.When;
            // todo: header.contentlocation
            //headers.ContentLocation = entry.Key.ToUri(Localhost.Base); dit moet door de exporter gezet worden.

            var resource = _entry.Resource as Binary;
            if (resource != null)
            {
                var binary = resource;
                headers.ContentType = new MediaTypeHeaderValue(binary.ContentType);
            }
        }

        public override bool CanReadType(Type type)
        {

            var can = typeof(Resource).IsAssignableFrom(type);  /* || type == typeof(Bundle) || (type == typeof(TagList) ) */ 
            return can;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(Resource).IsAssignableFrom(type);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            SetEntryHeaders(headers);
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            _entry = request.GetEntry();
            RequestMessage = request;
            return base.GetPerRequestFormatterInstance(type, request, mediaType);
        }

        protected string ReadBodyFromStream(Stream readStream, HttpContent content)
        {
            var charset = content.Headers.ContentType.CharSet ?? Encoding.UTF8.HeaderName;
            var encoding = Encoding.GetEncoding(charset);

            if (!Equals(encoding, Encoding.UTF8))
                throw Error.BadRequest("FHIR supports UTF-8 encoding exclusively, not " + encoding.WebName);

            var reader = new StreamReader(readStream, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }

    }

}