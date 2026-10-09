using TokenApp.Models;

namespace TokenApp.Services.Data
{
    /// <summary>
    /// Sign-in/out abstraction for ViewModels. Hides the API service and secure token storage.
    /// Login is inherently online: there is no offline fallback for credential verification.
    /// </summary>
    public interface IAuthRepository
    {
        Task<ServiceResult<AdminUser>> LoginAsync(string username, string password, CancellationToken ct = default);
        Task LogoutAsync();
        Task<bool> IsSignedInAsync();
    }
}
