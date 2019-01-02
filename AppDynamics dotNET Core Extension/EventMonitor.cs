using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using Microsoft.Web.Administration;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace AppDynamics.dotnetCore.Extension
{
    public class EventMonitor
    {
        public bool started = false;
        List<string> monitoredAppPools = null; 
        List<string> monitoredApps = null;
        StreamWriter logWriter = null;

        bool console =  Boolean.Parse(ConfigurationManager.AppSettings["ConsoleOutput"]);
        bool api = Boolean.Parse(ConfigurationManager.AppSettings["APIOutput"]);
        Dictionary<string, List<int>> appPools = new Dictionary<string, List<int>>();
        Uri targetUri = new Uri(ConfigurationManager.AppSettings["AnalyticsListener"]);

        const string metricPathMemory = "Custom Metrics|Nodes|{0}|Memory|{1}|{2}";
        const string metricPathProcess = "Custom Metrics|Nodes|{0}|Process|{1}|{2}";
        
        string processToReport = String.Empty;

        List<MetricData> performanceMetrics = new List<MetricData>();
        string DEBUG_LEVEL = ConfigurationManager.AppSettings["DebugLevel"].ToUpperInvariant();

        public EventMonitor(){ }

        public void Log(string msg)
        {
            string logPath = ConfigurationManager.AppSettings["DebugLogPath"];
            string debugLevel = ConfigurationManager.AppSettings["DebugLevel"].ToUpperInvariant();
            if (debugLevel.ToUpper() == "DEBUG")
            {
                using (logWriter = new StreamWriter(logPath, true))
                {
                    logWriter.WriteLine($"{DateTime.Now.ToString("mm/dd/yyy HH:mm")}\t{msg}");
                }
            }
        }

        private MetricData CreateMetricPackage(string metric, long value)
        {
            Log($"Creating Metric Package {metric}={value}");
            return new MetricData() { aggregatorType = "AVERAGE", metricName = metric, value = value };
        }

        public void Init()
        {
            monitoredAppPools = ConfigTools.GetConfiguredAppPools();
            monitoredApps = ConfigTools.GetConfiguredApps();
            console = Boolean.Parse(ConfigurationManager.AppSettings["ConsoleOutput"]);
            api = Boolean.Parse(ConfigurationManager.AppSettings["APIOutput"]);
            Log($"Configuration console:{console}, api:{api}");

        }

        public void Run() //List<string> monitoredAppPools, List<string> monitoredApps, bool console, bool api)
        {
            Log("Run method executed");
            started = true;

            using (var userSession = new TraceEventSession("ObserveGCAllocs"))
            {

                // enable the CLR provider with default keywords (minus the rundown CLR events)
                userSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose,
                    (ulong)(ClrTraceEventParser.Keywords.GC));
               
                // Create a stream of GC Allocation events (happens every time 100K of allocations happen)
                IObservable<GCAllocationTickTraceData> gcAllocStream = userSession.Source.Clr.Observe<GCAllocationTickTraceData>();
                
                // Create a stream of GC Collection events
                IObservable<GCHeapStatsTraceData> gcCollectStream = userSession.Source.Clr.Observe<GCHeapStatsTraceData>();

                // Print the outgoing stream to the console
                gcCollectStream.Subscribe(collectData =>
                {
                    
                    var appName = GetProcessName(collectData.ProcessID);
                    var appPoolName = GetAppPoolName(collectData.ProcessID);
                    var metricsList = new List<MetricData>();

                    if (monitoredApps.Contains(appName))
                     {

                        int pid = collectData.ProcessID;
                        Process toMonitor = Process.GetProcessById(pid);
                        long memoryUsed = toMonitor.WorkingSet64 /1000;
                        long memoryCommitted = toMonitor.PeakWorkingSet64/1000;
                        int threadCount = toMonitor.Threads.Count;
                        int handleCount = toMonitor.HandleCount;
                        var machineName = System.Environment.MachineName;


                        processToReport = String.Empty;
                        if (monitoredApps.Contains(appName))
                        {
                            processToReport = appName;
                        }

                        if (processToReport != String.Empty)
                        {
                            
                            if (api)
                            {
                                
                                metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 0 Usage", collectData.GenerationSize0));
                                metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 1 Usage", collectData.GenerationSize1)); 
                                metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 2 Usage", collectData.GenerationSize2));
                                metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Large Object Heap - Current Usage", collectData.GenerationSize3));
                                //metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Current Usage", memoryUsed));
                                //metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Current Committed", memoryCommitted));
                                //metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Process|{processToReport}|Thread Count", threadCount));
                                //metricsList.Add(CreateMetricPackage($"Custom Metrics|Nodes|{machineName}|Process|{processToReport}|Handles", handleCount));

                            }
                            if (console)
                            {
                                Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 0 Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={collectData.GenerationSize0}"); 
                                Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 1 Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={collectData.GenerationSize1}");
                                Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Memory Heap - Gen 2 Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={collectData.GenerationSize2}");
                                Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Large Object Heap - Current Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={collectData.GenerationSize3}");
                                // Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Current Usage,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={memoryUsed}");
                                // Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Memory|{processToReport}|Current Committed,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={memoryCommitted}");
                                // Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Process|{processToReport}|Thread Count,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={threadCount}");
                                // Console.WriteLine($"name=Custom Metrics|Nodes|{machineName}|Process|{processToReport}|Handles,aggregator=OBSERVATION,time-rollup=AVERAGE,cluster-rollup=INDIVIDUAL,value={handleCount}");
                            }
                        }

                    }


                    if (metricsList.Count > 0)
                        WriteLines(metricsList);
                });


                // OK we are all set up, time to listen for events and pass them to the observers.  
                userSession.Source.Process();
            }
        }

        private bool DoesProcessIdExist(int processID)
            {
                var result = false;
                var processName = String.Empty;
                foreach (var ap in appPools)
                {
                    result = ap.Value.Contains(processID);

                    // check for executable process name
                    if (!result)
                    {
                        processName = GetProcessName(processID);
                        result = (processName != String.Empty && processName != null);
                    }

                }
                return result;
            }
            private string GetAppPoolName(int processID)
            {
                var result = string.Empty;
                foreach (var ap in appPools)
                {
                    if (ap.Value.Contains(processID))
                    {
                        return ap.Key;
                    }
                }
                return result;
            }
            /// <summary>
            /// Returns the process name for a given process ID
            /// </summary>
            private string GetProcessName(int processID)
            {
                // Only keep the cache for 10 seconds to avoid issues with process ID reuse.  
                var now = DateTime.UtcNow;
                if ((now - s_processNameCacheLastUpdate).TotalSeconds > 10)
                    s_processNameCache.Clear();
                s_processNameCacheLastUpdate = now;

                string ret = null;
                if (!s_processNameCache.TryGetValue(processID, out ret))
                {
                    Process proc = null;
                    try
                    {
                        proc = Process.GetProcessById(processID);

                        if (proc != null)
                            ret = proc.ProcessName;
                        if (string.IsNullOrWhiteSpace(ret))
                            ret = processID.ToString();
                        s_processNameCache.Add(processID, ret);
                    }
                    catch (Exception) { }
                    finally { proc.Dispose(); }
                }
                return ret;
            }
            private float GetProcessCPU(int processID)
            {

                var process = Process.GetProcessById(processID);
                var total_cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                var process_cpu = new PerformanceCounter("Process", "% Processor Time", GetProcessName(processID));
                float processUsage = float.MinValue;
                try
                {
                    processUsage = (total_cpu.NextValue() / 100) * process_cpu.NextValue(); // process_cpu.NextValue() / Environment.ProcessorCount;
                                                                                            //var process_cpu_usage = (total_cpu.NextValue() / 100) * process_cpu.NextValue();
                }
                finally
                {
                    total_cpu.Dispose();
                    process_cpu.Dispose();
                }
                return processUsage;
            }
            private Dictionary<int, string> s_processNameCache = new Dictionary<int, string>();
            private DateTime s_processNameCacheLastUpdate;


            private async void WritetoAppD(List<MetricData> myMetrics)
            {
                try
                {
                    using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
                    {
                        if (client.BaseAddress != targetUri)
                        {
                            client.BaseAddress = targetUri;
                        }
                        
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                        using (HttpResponseMessage response = await client.PostAsJsonAsync(@"api/v1/metrics", myMetrics.ToArray()))
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            Log($"Status Code:{response.StatusCode}\tResponse Body: {responseBody}");
                      
                            foreach (var item in myMetrics)
                            {
                                Log($"Metrics sent to API : {item.metricName} \nValue : {item.value}");
                            }
                            Log($"Sent {myMetrics.Count} metrics");

                        }
                    }
                }
                catch (Exception ex)
                {
                    if (DEBUG_LEVEL == "INFO" | DEBUG_LEVEL == "DEBUG")
                        Log($"Error : {ex.Message}");
                }

            }
            private void WriteLines(List<MetricData> metrics)
            {
                WritetoAppD(metrics);
            }


            #region Console CtrlC handling
            private bool s_bCtrlCExecuted;
            private ConsoleCancelEventHandler s_CtrlCHandler;
            /// <summary>
            /// This implementation allows one to call this function multiple times during the
            /// execution of a console application. The CtrlC handling is disabled when Ctrl-C 
            /// is typed, one will need to call this method again to re-enable it.
            /// </summary>
            /// <param name="action"></param>
            private void SetupCtrlCHandler(Action action)
            {
                s_bCtrlCExecuted = false;
                // uninstall previous handler
                if (s_CtrlCHandler != null)
                    Console.CancelKeyPress -= s_CtrlCHandler;

                s_CtrlCHandler =
                    (object sender, ConsoleCancelEventArgs cancelArgs) =>
                    {
                        if (!s_bCtrlCExecuted)
                        {
                            s_bCtrlCExecuted = true;    // ensure non-reentrant

                            Log("Stopping monitor");

                            action();                   // execute custom action

                        // terminate normally (i.e. when the monitoring tasks complete b/c we've stopped the sessions)
                        cancelArgs.Cancel = true;
                        }
                    };
                Console.CancelKeyPress += s_CtrlCHandler;
            }            
    }

}
#endregion