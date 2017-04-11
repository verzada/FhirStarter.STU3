/* 
 * Copyright (c) 2016, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Hl7.Fhir.Model;

namespace Spark.Engine.Filters 
{
    /// <summary>
    ///   A GZip encoder/decoder for a HTTP messages.
    /// </summary>
    public class CompressionHandler : DelegatingHandler
    {
        public CompressionHandler(long maxDecompressedBodySizeInBytes = 1048576)
        {
            _compressors = new Dictionary<string, Func<HttpContent, HttpContent>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "gzip",  c => new GZipContent(c) }
        };
            _decompressors = new Dictionary<string, Func<HttpContent, HttpContent>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "gzip",  c => new GZipCompressedContent(c, maxDecompressedBodySizeInBytes) }
        };
        }

        /// <summary>
        ///  The MIME types that will not be compressed.
        /// </summary>
        readonly string[] _mediaTypeBlacklist = {
            "image/", "audio/", "video/",
            "application/x-rar-compressed",
            "application/zip", "application/x-gzip"
        };

        /// <summary>
        ///   The compressors that are supported.
        /// </summary>
        /// <remarks>
        ///   The key is the value of an "Accept-Encoding" HTTP header.
        /// </remarks>
        readonly Dictionary<string, Func<HttpContent, HttpContent>> _compressors;

        /// <summary>
        ///   The decompressors that are supported.
        /// </summary>
        /// <remarks>
        ///   The key is the value of an "Content-Encoding" HTTP header.
        /// </remarks>
        readonly Dictionary<string, Func<HttpContent, HttpContent>> _decompressors;

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Decompress the request content, if needed.
            if (request.Content != null && request.Content.Headers.ContentEncoding.Count > 0)
            {
                var encoding = request.Content.Headers.ContentEncoding.First();
                Func<HttpContent, HttpContent> decompressor;
                if (!_decompressors.TryGetValue(encoding, out decompressor))
                {
                    var outcome = new OperationOutcome
                    {
                        Issue = new List<OperationOutcome.IssueComponent>
                        {
                            new OperationOutcome.IssueComponent
                            {
                                Code = OperationOutcome.IssueType.NotSupported,
                                Details = new CodeableConcept("http://hl7.org/fhir/ValueSet/operation-outcome", "MSG_BAD_FORMAT",
                                    $"The Content-Encoding '{encoding}' is not supported."),
                                Severity = OperationOutcome.IssueSeverity.Error
                            }
                        }
                    };
                    throw new HttpResponseException(request.CreateResponse(HttpStatusCode.BadRequest, outcome));
                }
                try
                {
                    request.Content = decompressor(request.Content);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    var outcome = new OperationOutcome
                    {
                        Issue = new List<OperationOutcome.IssueComponent>
                        {
                            new OperationOutcome.IssueComponent
                            {
                                Code = OperationOutcome.IssueType.Forbidden,
                                Details = new CodeableConcept("http://hl7.org/fhir/ValueSet/operation-outcome", "MSG_BAD_FORMAT", ex.Message),
                                Severity = OperationOutcome.IssueSeverity.Error
                            }
                        }
                    };
                    throw new HttpResponseException(request.CreateResponse(HttpStatusCode.Forbidden, outcome));
                }
            }

            // Wait for the response.
            var response = await base.SendAsync(request, cancellationToken);


            // Is the media type blacklisted; because compression does not help?
            if (response?.Content?.Headers.ContentType == null || _mediaTypeBlacklist.Any(s => response.Content.Headers.ContentType.MediaType.StartsWith(s)))
                return response;

            // If the client has requested compression and the compression algorithm is known, 
            // then compress the response.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var compressor = request.Headers.AcceptEncoding?.Where(e => !e.Quality.HasValue || e.Quality != 0)
                .Where(e => _compressors.ContainsKey(e.Value))
                .OrderByDescending(e => e.Quality ?? 1.0)
                .Select(e => _compressors[e.Value])
                .FirstOrDefault();
            if (compressor != null)
            {
                response.Content = compressor(response.Content);
            }

            return response;
        }
    }
}
