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
    public class appPoolConfig : System.Configuration.ConfigurationElement
    {
        [ConfigurationProperty("metricName", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["metricName"];
            }
        }
    }

    public class appPoolsConfig : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("appPool")]
        public appPoolConfig AppPool
        {
            get { return (appPoolConfig)this["AppPool"]; }
        }
    }
}