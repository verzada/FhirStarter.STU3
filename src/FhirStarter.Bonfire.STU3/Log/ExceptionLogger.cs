using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Configuration;

namespace FhirStarter.Bonfire.STU3.Log
{
    public static class ExceptionLogger
    {
        //http://stackoverflow.com/questions/1091853/error-message-unable-to-load-one-or-more-of-the-requested-types-retrieve-the-l
        public static void LogReflectionTypeLoadException(ReflectionTypeLoadException ex)
        {
            var sb = new StringBuilder();
            foreach (var exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                var exFileNotFound = exSub as FileNotFoundException;
                if (!string.IsNullOrEmpty(exFileNotFound?.FusionLog))
                {
                    sb.AppendLine("Fusion Log:");
                    sb.AppendLine(exFileNotFound.FusionLog);
                }
                sb.AppendLine();
            }
            var errorMessage = sb.ToString();
            WriteLog(errorMessage, GetLogLocation());
            Console.WriteLine(errorMessage);
        }

        private static string GetLogLocation()
        {
            var logLocation = WebConfigurationManager.AppSettings["LogLocation"];
            if (string.IsNullOrEmpty(logLocation))
            {
                throw new ArgumentException("Need to add LogLocation to appSettings in web.config. Ex: " + @"C:\temp\log.log");
            }
            return logLocation;
        }

        private static void WriteLog(string log, string logLocation)
        {
            File.WriteAllText(@logLocation, log);
        }
    }
}

