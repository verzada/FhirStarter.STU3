using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FhirStarter.Logger.NetStandard
{
    public abstract class MessageHandler : DelegatingHandler
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var totalMill = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
            var corrId = $"{DateTime.Now.Ticks}{Thread.CurrentThread.ManagedThreadId}";
            var requestInfo = $"{request.Method} {request.RequestUri}";

            var requestMessage = await request.Content.ReadAsByteArrayAsync();            
            var hostname = request.RequestUri;
                       

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode)
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            else
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

            await OutgoingMessageAsync(corrId, requestInfo, responseMessage, requestMessage, hostname, totalMill, response);

            return response;
        }
      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="requestInfo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message,
            byte[] request, Uri hostname, double starttime, HttpResponseMessage responseMessage);
    }
}
