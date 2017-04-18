using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirStarter.Bonfire.STU3.Filter
{
    public abstract class AbstractExceptionFilter : ExceptionFilterAttribute
    {

        public override void OnException(HttpActionExecutedContext context)
        {
            var exceptionType = context.Exception.GetType();
            var expectedType = GetExceptionType();
            if (exceptionType != expectedType && !(expectedType == typeof(Exception))) return;
            var outCome = GetOperationOutCome(context.Exception);

            var xml = FhirSerializer.SerializeResourceToXml(outCome);
            var xmlDoc = XDocument.Parse(xml);

            context.Response = new HttpResponseMessage
            {
                Content = new StringContent(xmlDoc.ToString(), Encoding.UTF8, "application/xml"),
                StatusCode = HttpStatusCode.InternalServerError
            };

        }

        protected abstract Resource GetOperationOutCome(Exception exception);

        protected abstract Type GetExceptionType();
    }
}
