using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace AppDynamics.dotnetCore.Extension
{
    public static class ConfigTools
    {
        public static void Log(string msg)
        {
            string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
            StreamWriter logWriter;
            using (logWriter = new StreamWriter(logPath, true))
            {
                logWriter.WriteLine(msg);
            }
        }

        public static List<string> GetConfiguredAppPools()
        {

            var result = new List<string>();
            var appPoolSettings = ConfigurationManager.GetSection("MonitorSettings/appPools") as NameValueCollection;
            if (appPoolSettings.Count == 0)
            {
                Log("AppPool Settings are not defined");
                return new List<string>();
            }
            else
            {
                foreach (var key in appPoolSettings.AllKeys)
                {
                    var isMonitored = appPoolSettings[key];
                    if (isMonitored.ToLowerInvariant() == "true")
                    {
                        result.Add(key);
                    }
                }
            }
            return result;
        }

        public static List<string> GetConfiguredApps()
        {
            var result = new List<string>();
            var appSettings = ConfigurationManager.GetSection("MonitorSettings/Applications") as NameValueCollection;
            if (appSettings.Count == 0)
            {
                Log("App Settings are not defined");
                return new List<string>();
                //throw new NotImplementedException("No applications are defined for this application to monitor....");
            }
            else
            {
                foreach (var key in appSettings.AllKeys)
                {
                    var isMonitored = appSettings[key];
                    if (isMonitored.ToLowerInvariant() == "true")
                    {
                        result.Add(key);
                    }
                }
            }
            return result;
        }
    }
}