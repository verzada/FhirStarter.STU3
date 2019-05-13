/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Task = System.Threading.Tasks.Task;

namespace Spark.Engine.Formatters
{
    public class JsonFhirFormatter : FhirMediaTypeFormatter
    {
        public JsonFhirFormatter()
        {
            foreach (var mediaType in ContentType.JSON_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }
        
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = FhirMediaType.GetMediaTypeHeaderValue(type, ResourceFormat.Json);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew<object>(() => 
            {
                try
                {
                    var body = ReadBodyFromStream(readStream, content);

                    if (typeof(Resource).IsAssignableFrom(type))
                    {
                        var fhirparser = new FhirJsonParser();
                        var resource = fhirparser.Parse(body, type);
                        return resource;
                    }
                    throw Error.Internal("Cannot read unsupported type {0} from body", type.Name);
                }
                catch (FormatException exception)
                {
                    throw Error.BadRequest("Body parsing failed: " + exception.Message);
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                using(var streamwriter = new StreamWriter(writeStream))
                using (JsonWriter writer = new JsonTextWriter(streamwriter))
                {
                    var summary = RequestMessage.RequestSummary();
                    var jsonSerializer = new FhirJsonSerializer();

                    if (type == typeof(OperationOutcome))
                    {
                        Resource resource = (Resource)value;
                        //FhirSerializer.SerializeResource(resource, writer);
                        jsonSerializer.Serialize(resource,writer);
                    }
                    else if (typeof(Resource).IsAssignableFrom(type))
                    {
                        var resource = (Resource)value;
                        //FhirSerializer.SerializeResource(resource, writer);
                        jsonSerializer.Serialize(resource, writer);
                    }
                    else if (typeof(FhirResponse).IsAssignableFrom(type))
                    {
                        if (value is FhirResponse response && response.HasBody)
                        {
                            //FhirSerializer.SerializeResource(response.Resource, writer, summary);
                            jsonSerializer.Serialize(response.Resource, writer, summary);
                        }
                    }
                  
                }
            });
        }
    }
}
