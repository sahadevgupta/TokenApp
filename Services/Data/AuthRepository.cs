using TokenApp.Models;
using TokenApp.Services.Api;
using TokenApp.Services.Auth;

namespace TokenApp.Services.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ITokenApiService _api;
        private readonly ISessionService _session;

        public AuthRepository(ITokenApiService api, ISessionService session)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<ServiceResult<AdminUser>> LoginAsync(string username, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return ServiceResult<AdminUser>.Fail(FailureKind.Validation, "Username and password are required.");
            }

            var result = await _api.LoginAsync(username.Trim(), password, ct);
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult<AdminUser>.Fail(result.Failure, result.Message);
            }

            await _session.StartSessionAsync(result.Data.Token);
            return ServiceResult<AdminUser>.Ok(result.Data.Admin, result.Message);
        }

        public Task LogoutAsync() => _session.ClearAsync();

        public Task<bool> IsSignedInAsync() => _session.HasSessionAsync();
    }
}
