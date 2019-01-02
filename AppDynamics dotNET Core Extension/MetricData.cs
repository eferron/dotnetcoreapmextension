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
    public class MetricData
    {
        public string metricName;
        public string aggregatorType;
        public long value;
    }
}