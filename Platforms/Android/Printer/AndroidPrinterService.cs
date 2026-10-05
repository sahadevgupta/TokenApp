#if ANDROID
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Net.Wifi;
using Android.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using TokenApp.Interfaces;
using TokenApp.Models;

namespace TokenApp.Platforms.Android.Printer
{
    public class AndroidPrinterService : IPrinterService
    {
        readonly ILogger<AndroidPrinterService> _logger;
        TcpClient? _client;
        NetworkStream? _stream;
        bool _isConnected;

        public AndroidPrinterService(ILogger<AndroidPrinterService> logger)
        {
            _logger = logger;
        }

        public bool IsConnected => _isConnected && _client != null && _client.Connected;

        PrinterSettings LoadSettings()
        {
            var s = new PrinterSettings
            {
                IpAddress = Preferences.Get("Printer:IpAddress", string.Empty),
                Port = Preferences.Get("Printer:Port", 9100),
                EndpointUrl = Preferences.Get("Printer:EndpointUrl", string.Empty),
                UseHttp = Preferences.Get("Printer:UseHttp", false),
                RequiredSsid = Preferences.Get("Printer:RequiredSsid", string.Empty),
                RequireSsidMatch = Preferences.Get("Printer:RequireSsidMatch", true),
                ConnectTimeoutMs = Preferences.Get("Printer:ConnectTimeoutMs", 5000),
                WriteTimeoutMs = Preferences.Get("Printer:WriteTimeoutMs", 5000),
                HttpTimeoutMs = Preferences.Get("Printer:HttpTimeoutMs", 5000),
            };
            return s;
        }

        async Task<bool> ValidateNetworkAsync(string? requiredSsid, CancellationToken cancellationToken)
        {
            try
            {
                var context = global::Android.App.Application.Context;
                var connManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                var active = connManager.ActiveNetworkInfo;
                if (active == null || !active.IsConnected || active.Type != ConnectivityType.Wifi)
                {
                    _logger.LogWarning("Device is not connected to Wi-Fi");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(requiredSsid))
                    return true;

                var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
                var info = wifiManager.ConnectionInfo;
                if (info == null || string.IsNullOrEmpty(info.SSID))
                {
                    _logger.LogWarning("Unable to retrieve Wi-Fi SSID. Missing permission or service.");
                    return false;
                }

                var ssid = info.SSID.Trim('"');
                _logger.LogInformation("Connected SSID: {ssid}", ssid);
                return string.Equals(ssid, requiredSsid, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when validating network/ssid");
                return false;
            }
        }

        public async Task<bool> ConnectAsync(string ipAddress, int port = 9100, CancellationToken cancellationToken = default)
        {
            var settings = LoadSettings();
            var requiredSsid = settings.RequiredSsid;
            if (settings.RequireSsidMatch && !string.IsNullOrWhiteSpace(requiredSsid))
            {
                var ok = await ValidateNetworkAsync(requiredSsid, cancellationToken).ConfigureAwait(false);
                if (!ok)
                {
                    _logger.LogWarning("Device not on required Wi-Fi network ({ssid})", requiredSsid);
                    return false;
                }
            }

            // If HTTP endpoint is configured or UseHttp is true, validate endpoint reachability instead of raw TCP
            if (settings.UseHttp || !string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
                {
                    _logger.LogWarning("Printer endpoint URL not configured");
                    return false;
                }

                try
                {
                    using var http = new System.Net.Http.HttpClient();
                    http.Timeout = TimeSpan.FromMilliseconds(settings.HttpTimeoutMs);
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(settings.HttpTimeoutMs);
                    _logger.LogInformation("Checking printer HTTP endpoint {url}", settings.EndpointUrl);
                    var resp = await http.GetAsync(settings.EndpointUrl, cts.Token).ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode)
                    {
                        _isConnected = true;
                        _logger.LogInformation("Printer HTTP endpoint reachable");
                        return true;
                    }
                    _logger.LogWarning("Printer HTTP endpoint check returned {code}", resp.StatusCode);
                    return false;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("HTTP endpoint check timed out");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reach printer HTTP endpoint");
                    return false;
                }
            }

            var targetIp = string.IsNullOrWhiteSpace(ipAddress) ? settings.IpAddress : ipAddress;
            var targetPort = port == 0 ? settings.Port : port;
            if (string.IsNullOrWhiteSpace(targetIp))
            {
                _logger.LogWarning("No printer IP configured");
                return false;
            }

            try
            {
                _client?.Close();
                _client = new TcpClient();
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(settings.ConnectTimeoutMs);
                _logger.LogInformation("Connecting to printer {ip}:{port}", targetIp, targetPort);
                await _client.ConnectAsync(targetIp, targetPort).WaitAsync(cts.Token).ConfigureAwait(false);
                _stream = _client.GetStream();
                _isConnected = true;
                _logger.LogInformation("Connected to printer");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Connection to printer timed out");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to printer");
                return false;
            }
        }

        public async Task<bool> PrintTextAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var settings = LoadSettings();

            // If HTTP endpoint is configured, post text to it
            if (settings.UseHttp || !string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
                {
                    _logger.LogWarning("Printer endpoint URL not configured");
                    return false;
                }

                try
                {
                    using var http = new System.Net.Http.HttpClient();
                    http.Timeout = TimeSpan.FromMilliseconds(settings.HttpTimeoutMs);
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(settings.HttpTimeoutMs);
                    var content = new System.Net.Http.StringContent(text, Encoding.UTF8, "text/plain");
                    _logger.LogInformation("Posting print text to endpoint {url}", settings.EndpointUrl);
                    var resp = await http.PostAsync(settings.EndpointUrl, content, cts.Token).ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Print text posted successfully");
                        return true;
                    }
                    _logger.LogWarning("Printer endpoint returned {code} when posting text", resp.StatusCode);
                    return false;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("HTTP print operation timed out");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to POST print text to endpoint");
                    return false;
                }
            }

            // Fallback to TCP raw printing
            if (!IsConnected)
            {
                if (string.IsNullOrWhiteSpace(settings.IpAddress))
                {
                    _logger.LogWarning("Printer IP not configured");
                    return false;
                }

                var connected = await ConnectAsync(settings.IpAddress, settings.Port, cancellationToken).ConfigureAwait(false);
                if (!connected)
                    return false;
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(text + "\n");
                if (_stream == null) return false;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(settings.WriteTimeoutMs);
                await _stream.WriteAsync(data, 0, data.Length, cts.Token).ConfigureAwait(false);
                await _stream.FlushAsync(cts.Token).ConfigureAwait(false);
                _logger.LogInformation("Print text sent");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Print operation timed out");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send print text");
                return false;
            }
        }

        public async Task<bool> PrintBytesAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null || data.Length == 0)
                return false;

            var settings = LoadSettings();

            // If HTTP endpoint is configured, POST bytes to it
            if (settings.UseHttp || !string.IsNullOrWhiteSpace(settings.EndpointUrl))
            {
                if (string.IsNullOrWhiteSpace(settings.EndpointUrl))
                {
                    _logger.LogWarning("Printer endpoint URL not configured");
                    return false;
                }

                try
                {
                    using var http = new System.Net.Http.HttpClient();
                    http.Timeout = TimeSpan.FromMilliseconds(settings.HttpTimeoutMs);
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(settings.HttpTimeoutMs);
                    var content = new System.Net.Http.ByteArrayContent(data);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    _logger.LogInformation("Posting print bytes to endpoint {url}", settings.EndpointUrl);
                    var resp = await http.PostAsync(settings.EndpointUrl, content, cts.Token).ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Print bytes posted successfully");
                        return true;
                    }
                    _logger.LogWarning("Printer endpoint returned {code} when posting bytes", resp.StatusCode);
                    return false;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("HTTP print operation timed out");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to POST print bytes to endpoint");
                    return false;
                }
            }

            // Fallback to TCP raw printing
            if (!IsConnected)
            {
                var connected = await ConnectAsync(settings.IpAddress, settings.Port, cancellationToken).ConfigureAwait(false);
                if (!connected)
                    return false;
            }

            try
            {
                if (_stream == null) return false;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(settings.WriteTimeoutMs);
                await _stream.WriteAsync(data, 0, data.Length, cts.Token).ConfigureAwait(false);
                await _stream.FlushAsync(cts.Token).ConfigureAwait(false);
                _logger.LogInformation("Print bytes sent");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Print operation timed out");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send print bytes");
                return false;
            }
        }

        public Task DisconnectAsync()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                _isConnected = false;
                _logger.LogInformation("Disconnected from printer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }

            return Task.CompletedTask;
        }
    }
}
#endif
