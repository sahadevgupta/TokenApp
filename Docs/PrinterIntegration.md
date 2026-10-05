Printer Integration — Android (Wi‑Fi / Static URL)

Overview

This document describes end‑to‑end steps to enable printing from the MAUI Android app when the Android device and printer are on the same Wi‑Fi network (TCP/raw socket) or when the printer exposes an HTTP endpoint (static URL).

Files added or modified

- Models/PrinterSettings.cs — configuration model (IpAddress, Port, EndpointUrl, UseHttp, timeouts, RequiredSsid, etc.)
- Platforms/Android/Printer/AndroidPrinterService.cs — Android IPrinterService implementation
  - Validates Wi‑Fi connectivity and SSID
  - Supports HTTP (POST to a static endpoint) and TCP (raw socket) flows
  - Uses async I/O, CancellationToken, and timeouts
  - Logs via ILogger
- MauiProgram.cs — DI registration for AndroidPrinterService
- Interfaces/IPrinterService.cs — cross‑platform contract (existing)

Android manifest & permissions

Add the following permissions to Platforms/Android/AndroidManifest.xml:

- <uses-permission android:name="android.permission.INTERNET" />
- <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
- <uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
- <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />

Notes:
- ACCESS_FINE_LOCATION (or equivalent LocationWhenInUse) is required on newer Android to read SSID.
- Request runtime permission for location from UI (Activity/page) before attempting SSID validation.

Configuration (Preferences)

Store printer settings at runtime using Preferences (Microsoft.Maui.Storage). Example keys used by the service:

- Printer:IpAddress — printer IPv4 address (e.g., "192.168.1.100")
- Printer:Port — TCP port (default 9100)
- Printer:EndpointUrl — HTTP endpoint (e.g., "http://192.168.1.50:8080/print")
- Printer:UseHttp — boolean, prefer HTTP when true
- Printer:RequiredSsid — Wi‑Fi SSID that device must be connected to
- Printer:RequireSsidMatch — boolean, if true the service enforces RequiredSsid
- Printer:ConnectTimeoutMs — TCP connect timeout
- Printer:WriteTimeoutMs — TCP write timeout
- Printer:HttpTimeoutMs — HTTP request timeout

Provide a small settings page in the app to let administrators set these preferences.

AndroidPrinterService behavior

- LoadSettings(): reads PrinterSettings from Preferences.
- ValidateNetworkAsync(requiredSsid):
  - Confirms active network is Wi‑Fi (ConnectivityManager).
  - Reads Wi‑Fi SSID from WifiManager.ConnectionInfo and compares with RequiredSsid.
  - Returns false and logs if not on Wi‑Fi or SSID mismatch.
- ConnectAsync(ip, port):
  - If UseHttp or EndpointUrl configured: performs an HttpClient GET health check against EndpointUrl.
  - Else uses TcpClient to connect to printer IP/port and stores NetworkStream.
- PrintTextAsync(text) / PrintBytesAsync(bytes):
  - If HTTP endpoint configured: POSTs content to EndpointUrl (text/plain or application/octet-stream).
  - Else ensures TCP connection and writes bytes to NetworkStream.
- Error handling:
  - All operations are async and use CancellationToken.
  - Timeouts and exceptions result in a false return value and detailed logs — the service does not throw to UI.

How to call the service (ViewModel)

- Inject IPrinterService via DI into ViewModel.
- Before printing, ensure runtime location permission is granted on Android.
- Optionally call ConnectAsync(...) to preconnect and show status.
- Call PrintTextAsync or PrintBytesAsync and check the boolean result.
- Present friendly messages to users on failure.

Example (pseudo):

var ok = await _printerService.PrintTextAsync("Hello");
if (!ok) ShowMessage("Printer unavailable or not on required Wi‑Fi.");

Runtime permission flow (UI layer)

- Use Permissions API or platform checks in the UI to request LocationWhenInUse / ACCESS_FINE_LOCATION before printing.
- Flow:
  1. Check permission status.
  2. If not granted, request permission with rationale.
  3. On grant, proceed; on deny, show guidance to enable in settings.

Testing steps

TCP mode:
1. Connect Android device to the same Wi‑Fi SSID configured in RequiredSsid.
2. Ensure printer has static IP reachable on that network and port.
3. Set Printer:IpAddress and Printer:Port preferences.
4. Grant runtime location permission.
5. Trigger PrintTextAsync from the app and verify printed output.

HTTP (static URL) mode:
1. Connect device to required SSID if RequireSsidMatch=true.
2. Set Printer:EndpointUrl and Printer:UseHttp = true.
3. Trigger PrintTextAsync; verify the printer's web API receives the POST and prints.

Troubleshooting checklist

- "Device not connected to Wi‑Fi": ensure Wi‑Fi enabled and connected to correct network.
- "Unable to retrieve Wi‑Fi SSID": ensure runtime location permission granted and location services enabled.
- "Connection to printer timed out": verify IP/port, network reachability, same subnet, and printer network state.
- "Printer endpoint returned 4xx/5xx": check API contract, required headers, auth, and payload format.
- Use device logcat and the app's ILogger (Sentry) to collect details.

Extending for authentication/headers

- If the printer API requires auth or headers, extend PrinterSettings with fields for Authorization token and custom headers.
- Add these headers to HttpClient requests before POST.

Vendor AAR SDK notes

- The repository contains Platforms/Android/Printer/printer-lib-3.5.8.aar.
- To use vendor SDK:
  1. Create an Android Binding Library project (or include the AAR directly in the Android project with proper bindings).
  2. Map Java methods to C# via binding metadata.
  3. Call SDK methods from AndroidPrinterService for advanced features.
- If you want, provide SDK docs or Java sample usage and the binding can be created.

Security & production

- Use SecureStorage for sensitive tokens or credentials.
- Use authentication on the printer web API when available.
- Validate payload sizes and apply timeouts/backpressure for large jobs.

Quick checklist to get printing working now

1. Add manifest permissions (ACCESS_WIFI_STATE, ACCESS_FINE_LOCATION, INTERNET).
2. Provide a settings page to store printer preferences.
3. Request runtime location permission from UI before printing.
4. Trigger PrintTextAsync and handle boolean result.

Files in repo now

- Docs/PrinterIntegration.md (this file)

Generating a PDF

I included a PowerShell script in the Docs folder (MakePdf.ps1) that uses pandoc if available to convert this Markdown file to PDF. If you prefer I can generate the PDF in the repo directly, but conversion tools (pandoc, wkhtmltopdf, or headless Chromium) must be present on the environment.

If you want me to also produce a PDF file in the repo directly, confirm and I will attempt conversion using available tools in the environment or provide the PDF file as generated bytes.

---

