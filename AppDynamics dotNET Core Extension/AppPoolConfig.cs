using System.Configuration;

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