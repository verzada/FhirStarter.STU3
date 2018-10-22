using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FhirStarter.Flare.STU3.Log
{
     public class MessageLoggingHandler : MessageHandler
    {
        private readonly bool _logRequest;
        // ReSharper disable once MemberCanBePrivate.Global
        public static string LogRequestWhenError = nameof(LogRequestWhenError);
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logRequestWhenError">true / false value from app.settings or json settings</param>
        public MessageLoggingHandler(string logRequestWhenError)
        {
            _logRequest = IsRequestsLogged(logRequestWhenError, false);
        }

        /// <summary>
        /// Uses the ConfigurationManager to find the true / false setting for logging incoming requests. The setting in the web.config is LogRequestWhenError under AppSettings
        /// </summary>
        public MessageLoggingHandler()
        {
            _logRequest = IsRequestsLogged(ConfigurationManager.AppSettings[LogRequestWhenError], true);
        }

        private bool IsRequestsLogged(string settingsValue, bool isAppSettings)
        {
            if (string.IsNullOrEmpty(settingsValue))
            {
                Log.Warn(
                    isAppSettings
                        ? $"The config file does not contain the AppSetting {LogRequestWhenError}. It should be set to true if incoming requests needs to be logged"
                        : $"The incoming settings from the construct call {nameof(MessageLoggingHandler)} is empty or null. Incoming requests will not be logged by default.");

                return false;
            }
            
            var logRequests = Convert.ToBoolean(settingsValue);

            if (logRequests && isAppSettings)
            {
                Log.Info($"Incoming requests will be logged since {LogRequestWhenError} is set to true");
            }

            else if (logRequests)
            {
                Log.Info($"Incoming requests will be logged since the settings sent into the constructor method {nameof(MessageLoggingHandler)} is set to true");
            }

            else if (isAppSettings)
            {
                Log.Warn($"Incoming requests will not be logged since the appsetting {LogRequestWhenError} is set to false");
            }
            else
            {
                Log.Warn($"Incoming requests will not be logged since the constructor {nameof(MessageLoggingHandler)} call has a parameter that is null or false");
            }
            return logRequests;
        }


        /// <inheritdoc />
        protected override async Task OutgoingMessageAsync(string requestMethod, byte[] message, byte[] requestMessage, Uri hostname, double starttime, HttpResponseMessage responseMessage)
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
                            $"RequestMethod: {requestMethod} ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}");
                });
            }
            else
            {
                if (responseMessage.Content != null)
                {
                    var responseMessageDetailed = await GetUnzippedResponse(responseMessage);
                    try
                    {
                        var document = XDocument.Parse(responseMessageDetailed);
                        responseMessageDetailed = document.ToString();
                    }
                    finally
                    {
                        await Task.Run(() =>
                        {
                            Log.Error(!_logRequest
                                ? $"ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}; Outcome: {responseMessageDetailed};"
                                : $"ReqLength: {requestMessage.Length}; ResLength: {responseLength}; Hostname: {host}; Path: {hostname.PathAndQuery}; Elapsed: {diff}; Outcome: {responseMessageDetailed}; Request: {System.Text.Encoding.Default.GetString(requestMessage)};");
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Handling exceptions that arrive as a zipped stream
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private static async Task<string> GetUnzippedResponse(HttpResponseMessage responseMessage)
        {
            var responseStream = await responseMessage.Content.ReadAsStreamAsync();  

            // zipStream must be outside a 'using bracket' since it is not possible to close the zip stream within a 'using bracket'
            var zipStream = new GZipStream(responseStream, CompressionMode.Decompress);
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                var exceptionStream = resultStream.ToArray();
                return System.Text.Encoding.Default.GetString(exceptionStream);
            }                
        }       
    }
}
