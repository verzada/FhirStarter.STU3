using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Spark.Engine.Core;

namespace FhirStarter.Bonfire.STU3.Filter
{
    public abstract class AbstractExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnException(HttpActionExecutedContext context)
        {
            var exceptionType = context.Exception.GetType();
            var expectedType = GetExceptionType();
            var exceptionMessage = context.Exception.Message;

            if (exceptionType != expectedType && !(expectedType == typeof(Exception))) return;

            Resource operationOutcome = null;
            if (exceptionMessage.Contains("<" + nameof(OperationOutcome)))
            {
                var serializer = new FhirXmlParser();
                operationOutcome = serializer.Parse<OperationOutcome>(exceptionMessage);
            }
            var outCome = operationOutcome ?? GetOperationOutCome(context.Exception);

            var xml = FhirSerializer.SerializeResourceToXml(outCome);
            var xmlDoc = XDocument.Parse(xml);
            var error = xmlDoc.ToString();
            var htmlDecode = WebUtility.HtmlDecode(error);
            Log.Error(htmlDecode);
            SetResponseForClient(context, outCome);
        }

        private static void SetResponseForClient(HttpActionExecutedContext context, Resource outCome)
        {
            // "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"
            var acceptEntry = HttpContext.Current.Request.Headers["Accept"];
            var acceptJson = acceptEntry.Contains(FhirMediaType.HeaderTypeJson);

            if (acceptJson)
            {
                var json = FhirSerializer.SerializeToJson(outCome);
                context.Response = new HttpResponseMessage
                {
                    Content = new StringContent(json, Encoding.UTF8, FhirMediaType.JsonResource),
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
            else
            {
                var xml = FhirSerializer.SerializeToXml(outCome);
                context.Response = new HttpResponseMessage
                {
                    Content = new StringContent(xml, Encoding.UTF8, FhirMediaType.XmlResource),
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        protected abstract Resource GetOperationOutCome(Exception exception);

        protected abstract Type GetExceptionType();
    }
}
