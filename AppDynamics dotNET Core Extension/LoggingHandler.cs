using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace AppDynamics.dotnetCore.Extension
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        public void Log(string msg)
        {
            string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
            string debugLevel = ConfigurationManager.AppSettings["DebugLevel"].ToUpperInvariant();
            if (debugLevel.ToUpper() == "DEBUG")
            {
                using (StreamWriter logWriter = new StreamWriter(logPath, true))
                {
                    logWriter.WriteLine($"{DateTime.Now.ToString("mm/dd/yyy HH:mm")}\t{msg}");
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Log("Request:\r\n");
            Log($"{request.ToString()}\r\n");
            if (request.Content != null)
            {
                Log($"{await request.Content.ReadAsStringAsync()}\r\n");
            }


            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Log("\r\nResponse:\r\n");
            Log($"{response.ToString()}\r\n");
            if (response.Content != null)
            {
                Log($"{await response.Content.ReadAsStringAsync()}");
            }
            Log("Request:\r\n");

            return response;
        }
    }
}