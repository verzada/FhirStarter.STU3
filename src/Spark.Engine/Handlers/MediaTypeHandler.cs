/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Spark.Engine.Core;
using Spark.Engine.Extensions;

namespace Spark.Engine.Handlers
{
    public class FhirMediaTypeHandler : DelegatingHandler
    {
        private bool IsBinaryRequest(HttpRequestMessage request)
        {
            var ub = new UriBuilder(request.RequestUri);
            return ub.Path.Contains("Binary"); 
            // HACK: replace quick hack by solid solution.
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var formatParam = request.GetParameter("_format");
            if (!string.IsNullOrEmpty(formatParam))
            {
                var accepted = ContentType.GetResourceFormatFromFormatParam(formatParam);
                if (accepted != ResourceFormat.Unknown)
                {
                    request.Headers.Accept.Clear();

                    request.Headers.Accept.Add(accepted == ResourceFormat.Json
                        ? new MediaTypeWithQualityHeaderValue(ContentType.JSON_CONTENT_HEADER)
                        : new MediaTypeWithQualityHeaderValue(ContentType.XML_CONTENT_HEADER));
                }
            }

            // BALLOT: binary upload should be determined by the Content-Type header, instead of the Rest url?
            // HACK: passes to BinaryFhirFormatter
            if (IsBinaryRequest(request))
            {
                if (request.Content.Headers.ContentType != null)
                {
                    var format = request.Content.Headers.ContentType.MediaType;
                    request.Content.Headers.Replace("X-Content-Type", format);
                }

                request.Content.Headers.ContentType = new MediaTypeHeaderValue(FhirMediaType.BinaryResource);
                if (request.Headers.Accept.Count == 0)
                {
                    request.Headers.Replace("Accept", FhirMediaType.BinaryResource);
                }
            }
          
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
