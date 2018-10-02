using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FhirStarter.Logger.NetStandard
{
     public class MessageLoggingHandler : MessageHandler
    {
        private readonly bool _logRequest;
        public static string LogRequestWhenError = nameof(LogRequestWhenError);
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logRequestWhenError">true / false value from app.settings or json settings</param>
        public MessageLoggingHandler(string logRequestWhenError)
        {
            _logRequest = IsRequestsLogged(logRequestWhenError);
        }


        /// <summary>
        /// Uses the ConfigurationManager to find the true / false setting for logging incoming requests. The setting in the web.config is LogRequestWhenError
        /// </summary>
        public MessageLoggingHandler()
        {
            _logRequest = IsRequestsLogged(ConfigurationManager.AppSettings[LogRequestWhenError]);
        }

        private bool IsRequestsLogged(string settingsValue)
        {
            if (string.IsNullOrEmpty(settingsValue))
            {
                Log.Warn(
                    $"The config file does not contain the AppSetting {LogRequestWhenError}. It should be set to true if incoming requests needs to be logged");
                return false;
            }

            Log.Info($"Incoming requests will be logged since {LogRequestWhenError} is set to true");
            return Convert.ToBoolean(settingsValue);
        }


        /// <inheritdoc />
        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message, byte[] requestMessage, Uri hostname, double starttime, HttpResponseMessage responseMessage)
        {
            var host = hostname.Scheme + Uri.SchemeDelimiter + hostname.Host + ":" + hostname.Port;
            var responseLength = message.Length;
            var endMill = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
            var diff = endMill - starttime;

            if (responseMessage.IsSuccessStatusCode)
            {
                await Task.Run(() =>
                {
                    Log.Info(
                            $"ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}");
                });
            }
            else
            {
                if (responseMessage.Content != null)
                {
                    var exception = responseMessage.Content.ReadAsAsync<HttpError>();
                    await Task.Run(() =>
                    {
                        Log.Error(!_logRequest
                            ? $"ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}; Exception: {exception.Result.ExceptionMessage}; Stacktrace: {exception.Result.StackTrace}"
                            : $"ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}; Exception: {exception.Result.ExceptionMessage}; Request: {System.Text.Encoding.Default.GetString(requestMessage)}; Stacktrace: {exception.Result.StackTrace}");
                    });
                }
                
            }
        }
    }
}
