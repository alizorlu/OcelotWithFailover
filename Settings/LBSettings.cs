using System;

namespace Pretech.Ihale.Mobile.Gateway.Settings
{
    public class LBSettings
    {
        public Envanters[] Envanters { get; set; }
        public HttpRequestSettings HttpRequestSettings { get; set; }
    }
    public class Envanters
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
    public class HttpRequestSettings
    {
        public TimeSpan RequestTimeout { get; set; }
    }
}
