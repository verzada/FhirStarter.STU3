using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Spark.Engine.Core;

namespace Spark.Engine.Filters
{
    public class FhirResponseHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith(
                task =>
                {
                    if (task.IsCompleted)
                    {
                        FhirResponse fhirResponse;
                        if (task.Result.TryGetContentValue(out fhirResponse))
                        {
                            return request.CreateResponse(fhirResponse);
                        }
                        return task.Result;
                    }
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    //return task.Result;
                }, 
                cancellationToken
            );
        }
    }
}
