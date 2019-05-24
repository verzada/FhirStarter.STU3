using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Hl7.Fhir.Model;

namespace FhirStarter.Bonfire.STU3.Filter
{
    public class ExceptionFilter : AbstractExceptionFilter
    {
        //    private static readonly log4net.ILog log =
        //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ExceptionFilter()
        {
        }

        protected override Resource GetOperationOutCome(Exception exception)
        {
            LogError(exception);

            var operationOutCome = new OperationOutcome { Issue = new List<OperationOutcome.IssueComponent>() };
            var issue = new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Fatal,
                Code = OperationOutcome.IssueType.NotFound,
                Details = new CodeableConcept("StandardException", exception.GetType().ToString(), exception.Message),
            };

            if (ShowStackTraceInOperationOutcome())
            {
                issue.Diagnostics = exception.StackTrace;
            }

            var responseIssue = CheckForHttpResponseException(exception);
            if (responseIssue != null)
                operationOutCome.Issue.Add(responseIssue);

            operationOutCome.Issue.Add(issue);
            return operationOutCome;
        }

        private static bool ShowStackTraceInOperationOutcome()
        {
            var stackTraceValue = ConfigurationManager.AppSettings["ShowStacktraceInOperationOutcome"];
            if (!string.IsNullOrEmpty(stackTraceValue))
            {
                bool.TryParse(stackTraceValue, out var booleanResult);
                return booleanResult;
            }

            return false;
        }

        private static OperationOutcome.IssueComponent CheckForHttpResponseException(Exception exception)
        {
            OperationOutcome.IssueComponent responseIssue = null;
            if (exception.GetType().ToString().Contains(nameof(HttpResponseException)))
            {
                var responseException = (HttpResponseException)exception;

                if (responseException.Response != null)
                {
                    responseIssue = new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Fatal,
                        Code = OperationOutcome.IssueType.Exception,
                        Details =
                            new CodeableConcept("Response", exception.GetType().ToString(),
                                responseException.Response.ReasonPhrase)
                    };
                    if (ShowStackTraceInOperationOutcome())
                    {
                        responseIssue.Diagnostics = exception.StackTrace;
                    }
                }
            }
            return responseIssue;
        }

        // todo add support for post (needs examples for it)
        private void LogError(Exception exception)
        {
            var url = GetAbsolutePathRequest();
            // var post = GetPost();
            // var exceptionXml = XmlConverter.CreateXmlDocumentFromObject(exception);
            if (!string.IsNullOrEmpty(url))
            {
                var newXDoc = new XDocument();
                var root = new XElement("Error");

                var contentElement = new XElement("RequestUrl", url);
                root.Add(contentElement);
                newXDoc.Add(root);

            }
        }

        private static string GetAbsolutePathRequest()
        {
            var httpRequest = HttpContext.Current;
            var absolutePath = string.Empty;
            if (httpRequest != null)
            {
                absolutePath = httpRequest.Request.Url.ToString();
            }
            return absolutePath;
        }

        protected override Type GetExceptionType()
        {
            return typeof(Exception);
        }
    }
}
