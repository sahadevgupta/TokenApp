using System;

namespace TokenApp.Models
{
    public class PrinterSettings
    {
        // For raw TCP printing
        public string? IpAddress { get; set; }
        public int Port { get; set; } = 9100;

        // For HTTP-based printers (static URL)
        public string? EndpointUrl { get; set; }
        public bool UseHttp { get; set; } = false;
        public int HttpTimeoutMs { get; set; } = 5000;

        // Network requirements
        public string? RequiredSsid { get; set; }
        public bool RequireSsidMatch { get; set; } = true;

        // Timeouts for TCP
        public int ConnectTimeoutMs { get; set; } = 5000;
        public int WriteTimeoutMs { get; set; } = 5000;
    }
}
