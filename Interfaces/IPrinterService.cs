using System;
using System.Collections.Generic;
using System.Text;

namespace TokenApp.Interfaces
{
    public interface IPrinterService
    {
        Task<bool> ConnectAsync(string ipAddress,int port = 9100,CancellationToken cancellationToken = default);

        Task<bool> PrintTextAsync(
            string text,
            CancellationToken cancellationToken = default);

        Task<bool> PrintBytesAsync(
            byte[] data,
            CancellationToken cancellationToken = default);

        Task DisconnectAsync();

        bool IsConnected { get; }
    }
}
