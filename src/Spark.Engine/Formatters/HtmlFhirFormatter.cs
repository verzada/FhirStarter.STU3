/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Task = System.Threading.Tasks.Task;

// using Spark.Service;

namespace Spark.Engine.Formatters
{
    public class HtmlFhirFormatter : FhirMediaTypeFormatter
    {
        public HtmlFhirFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("text/html");
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew<object>(() =>
            {
                try
                {
                    throw new NotSupportedException($"Cannot read unsupported type {type.Name} from body");
                }
                catch (FormatException exc)
                {
                    throw Error.BadRequest("Body parsing failed: " + exc.Message);
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                WriteHtmlOutput(type, value, writeStream);
            });
        }

        private void WriteHtmlOutput(Type type, object value, Stream writeStream)
        {
            var writer = new StreamWriter(writeStream, Encoding.UTF8);
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            writer.WriteLine("  <link href=\"/Content/fhir-html.css\" rel=\"stylesheet\"></link>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            if (type == typeof(OperationOutcome))
            {
                var oo = (OperationOutcome)value;

                if (oo.Text != null)
                    writer.Write(oo.Text.Div);
            }
            else if (type == typeof(Resource))
            {
                var bundle = value as Bundle;
                if (bundle != null)
                {
                    var resource = bundle;

                    if (resource.SelfLink != null)
                    {
                        writer.WriteLine($"Searching: {resource.SelfLink.OriginalString}<br/>");

                        // Hl7.Fhir.Model.Parameters query = FhirParser.ParseQueryFromUriParameters(collection, parameters);

                        var ps = resource.SelfLink.ParseQueryString();
                        if (ps.AllKeys.Contains(FhirParameter.SORT))
                            writer.WriteLine($"    Sort by: {ps[FhirParameter.SORT]}<br/>");
                        if (ps.AllKeys.Contains(FhirParameter.SUMMARY))
                            writer.WriteLine("    Summary only<br/>");
                        if (ps.AllKeys.Contains(FhirParameter.COUNT))
                            writer.WriteLine($"    Count: {ps[FhirParameter.COUNT]}<br/>");
                        if (ps.AllKeys.Contains(FhirParameter.SNAPSHOT_INDEX))
                            writer.WriteLine($"    From RowNum: {ps[FhirParameter.SNAPSHOT_INDEX]}<br/>");
                        if (ps.AllKeys.Contains(FhirParameter.SINCE))
                            writer.WriteLine($"    Since: {ps[FhirParameter.SINCE]}<br/>");


                        foreach (var item in ps.AllKeys.Where(k => !k.StartsWith("_")))
                        {
                            if (ModelInfo.SearchParameters.Exists(s => s.Name == item))
                            {
                                writer.WriteLine($"    {item}: {ps[item]}<br/>");
                                //var parameters = transportContext..Request.TupledParameters();
                                //int pagesize = Request.GetIntParameter(FhirParameter.COUNT) ?? Const.DEFAULT_PAGE_SIZE;
                                //bool summary = Request.GetBooleanParameter(FhirParameter.SUMMARY) ?? false;
                                //string sortby = Request.GetParameter(FhirParameter.SORT);
                            }
                            else
                            {
                                writer.WriteLine($"    <i>{item}: {ps[item]} (excluded)</i><br/>");
                            }
                        }
                    }

                    if (resource.FirstLink != null)
                        writer.WriteLine($"First Link: {resource.FirstLink.OriginalString}<br/>");
                    if (resource.PreviousLink != null)
                        writer.WriteLine($"Previous Link: {resource.PreviousLink.OriginalString}<br/>");
                    if (resource.NextLink != null)
                        writer.WriteLine($"Next Link: {resource.NextLink.OriginalString}<br/>");
                    if (resource.LastLink != null)
                        writer.WriteLine($"Last Link: {resource.LastLink.OriginalString}<br/>");

                    // Write the other Bundle Header data
                    writer.WriteLine(
                        $"<span style=\"word-wrap: break-word; display:block;\">Type: {resource.Type.ToString()}, {resource.Entry.Count} of {resource.Total}</span>");

                    foreach (var item in resource.Entry)
                    {
                        //IKey key = item.ExtractKey();

                        writer.WriteLine("<div class=\"item-tile\">");
                        if (item.IsDeleted())
                        {
                            if (item.Request != null)
                            {
                                var id = item.Request.Url;
                                writer.WriteLine($"<span style=\"word-wrap: break-word; display:block;\">{id}</span>");
                            }
                            
                            //if (item.Deleted.Instant.HasValue)
                            //    writer.WriteLine(String.Format("<i>Deleted: {0}</i><br/>", item.Deleted.Instant.Value.ToString()));

                            writer.WriteLine("<hr/>");
                            writer.WriteLine("<b>DELETED</b><br/>");
                        }
                        else if (item.Resource != null)
                        {
                            var key = item.Resource.ExtractKey();
                            
                            var visualurl = key.WithoutBase().ToUriString();
                            var realurl = key.ToUriString() + "?_format=html";

                            writer.WriteLine(
                                $"<a style=\"word-wrap: break-word; display:block;\" href=\"{realurl}\">{visualurl}</a>");
                            if (item.Resource.Meta?.LastUpdated != null)
                                writer.WriteLine(
                                    $"<i>Modified: {item.Resource.Meta.LastUpdated.Value}</i><br/>");
                            writer.WriteLine("<hr/>");

                            var itemResource = item.Resource as DomainResource;
                            if (itemResource != null)
                            {
                                if (!string.IsNullOrEmpty(itemResource.Text?.Div))
                                    writer.Write(itemResource.Text.Div);
                                else
                                    writer.WriteLine($"Blank Text: {itemResource.ExtractKey().ToUriString()}<br/>");
                            }
                            else 
                            {
                                writer.WriteLine("This is not a domain resource");
                            }

                        }
                        writer.WriteLine("</div>");
                    }
                }
                else
                {
                    var resource = (DomainResource)value;
                    var org = resource.ResourceBase + "/" + resource.ResourceType + "/" + resource.Id;
                    // TODO: This is probably a bug in the service (Id is null can throw ResourceIdentity == null
                    // reference ResourceIdentity : org = resource.ResourceIdentity().OriginalString;
                    writer.WriteLine($"Retrieved: {org}<hr/>");

                    var text = resource.Text?.Div;
                    writer.Write(text);
                    writer.WriteLine("<hr/>");

                    var summary = RequestMessage.RequestSummary();
                    var xml = FhirSerializer.SerializeResourceToXml(resource, summary);
                    var xmlDoc = new System.Xml.XPath.XPathDocument(new StringReader(xml));

                    // And we also need an output writer
                    TextWriter output = new StringWriter(new StringBuilder());

                    // Now for a little magic
                    // Create XML Reader with style-sheet
                    var stylesheetReader = System.Xml.XmlReader.Create(new StringReader(Resources.RenderXMLasHTML));

                    var xslTransform = new System.Xml.Xsl.XslCompiledTransform();
                    xslTransform.Load(stylesheetReader);
                    xslTransform.Transform(xmlDoc, null, output);

                    writer.WriteLine(output.ToString());
                }
            }

            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
            writer.Flush();
        }
    }
}
