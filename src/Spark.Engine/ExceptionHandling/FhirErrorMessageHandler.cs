using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Hl7.Fhir.Model;

namespace Spark.Engine.ExceptionHandling
{
    public class FhirErrorMessageHandler : DelegatingHandler
    {

        // the error handling has been changed, look into this
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response =  await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = response.Content as ObjectContent;
                if (content != null && content.ObjectType == typeof (HttpError))
                {
                    var issue = new OperationOutcome.IssueComponent
                    {
                        Details = new CodeableConcept(nameof(HttpError), nameof(HttpError), response.ReasonPhrase)
                    };
                    var outcome = new OperationOutcome();
                    outcome.Issue.Add(issue);
                    
                    return request.CreateResponse(response.StatusCode, outcome);
                }
            }
            return response;
        }
    }
}