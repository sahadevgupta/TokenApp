using System.Net;
using System.Text.Json;
using Refit;
using TokenApp.Configurations.Mapper;
using TokenApp.Models;
using TokenApp.Models.Request;
using TokenApp.Models.Response;
using TokenApp.Services.Connectivity;

namespace TokenApp.Services.Api
{
    /// <summary>
    /// Application-facing API service. Owns every HTTP-level concern: connectivity pre-check,
    /// Refit call, status-code → <see cref="FailureKind"/> translation (per the API documentation),
    /// and mapping backend DTOs into app models. Callers never see a DTO or an HTTP exception.
    /// </summary>
    public interface ITokenApiService
    {
        Task<ServiceResult<LoginResult>> LoginAsync(string username, string password, CancellationToken ct = default);
        Task<ServiceResult<DashboardSummary>> GetDashboardAsync(CancellationToken ct = default);

        /// <summary>Generates + approves a token on the backend. The returned token carries the RemoteId.</summary>
        Task<ServiceResult<TokenDTO>> GenerateAndPrintTokenAsync(TokenDTO token, CancellationToken ct = default);

        Task<ServiceResult<TokenDTO>> ApplyTokenAsync(TokenType type, CancellationToken ct = default);
        Task<ServiceResult<TokenDTO>> GetPrintTokenAsync(int remoteId, CancellationToken ct = default);

        Task<ServiceResult> ApproveTokenAsync(int remoteId, CancellationToken ct = default);
        Task<ServiceResult> RejectTokenAsync(int remoteId, string reason, CancellationToken ct = default);
        Task<ServiceResult> DeleteTokenAsync(int remoteId, CancellationToken ct = default);
        Task<ServiceResult> ClearApprovedTokensAsync(CancellationToken ct = default);
        Task<ServiceResult> UpdateAdminPasswordAsync(int adminId, string newPassword, CancellationToken ct = default);
    }

    public class TokenApiService : ITokenApiService
    {
        private const string ForbiddenMessage = "You do not have permission for this action.";
        private const string SessionExpiredMessage = "Your session has expired. Please sign in again.";
        private const string OfflineMessage = "No internet connection.";
        private const string NetworkMessage = "Could not reach the server. Please try again.";
        private const string GenericMessage = "Something went wrong. Please try again.";

        private readonly ITokenBackendApi _api;
        private readonly IConnectivityService _connectivity;

        public TokenApiService(ITokenBackendApi api, IConnectivityService connectivity)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
        }

        public Task<ServiceResult<LoginResult>> LoginAsync(string username, string password, CancellationToken ct = default)
        {
            var request = new LoginRequestDto { Username = username, Password = password };

            return ExecuteAsync(
                () => _api.LoginAsync(request, ct),
                dto => string.IsNullOrWhiteSpace(dto.Token) ? null : BackendToAppModelMapper.GetLoginResult(dto),
                ct);
        }

        public Task<ServiceResult<DashboardSummary>> GetDashboardAsync(CancellationToken ct = default) =>
            ExecuteAsync(
                () => _api.GetDashboardAsync(ct),
                dto => dto.Data == null ? null : BackendToAppModelMapper.GetDashboard(dto.Data),
                ct);

        public Task<ServiceResult<TokenDTO>> GenerateAndPrintTokenAsync(TokenDTO token, CancellationToken ct = default)
        {
            var request = AppToBackendModelMapper.GetGenerateTokenRequest(token);
            if (request == null)
            {
                return Task.FromResult(ServiceResult<TokenDTO>.Fail(FailureKind.Validation, $"{token.Type} tokens cannot be generated."));
            }

            return ExecuteAsync(
                () => _api.GenerateAndPrintTokenAsync(request, ct),
                dto => dto.Data == null ? null : BackendToAppModelMapper.GetToken(dto.Data),
                ct);
        }

        public Task<ServiceResult<TokenDTO>> ApplyTokenAsync(TokenType type, CancellationToken ct = default)
        {
            var request = AppToBackendModelMapper.GetApplyTokenRequest(type);
            if (request == null)
            {
                return Task.FromResult(ServiceResult<TokenDTO>.Fail(FailureKind.Validation, $"{type} tokens cannot be applied for."));
            }

            return ExecuteAsync(
                () => _api.ApplyTokenAsync(request, ct),
                dto => dto.Data == null ? null : BackendToAppModelMapper.GetToken(dto.Data),
                ct);
        }

        public Task<ServiceResult<TokenDTO>> GetPrintTokenAsync(int remoteId, CancellationToken ct = default) =>
            ExecuteAsync(
                () => _api.GetPrintTokenAsync(remoteId, ct),
                dto => dto.Token == null ? null : BackendToAppModelMapper.GetToken(dto.Token),
                ct);

        public Task<ServiceResult> ApproveTokenAsync(int remoteId, CancellationToken ct = default) =>
            ExecuteAsync(() => _api.ApproveTokenAsync(remoteId, ct), ct);

        public Task<ServiceResult> RejectTokenAsync(int remoteId, string reason, CancellationToken ct = default) =>
            ExecuteAsync(() => _api.RejectTokenAsync(remoteId, new RejectTokenRequestDto { Reason = reason }, ct), ct);

        public Task<ServiceResult> DeleteTokenAsync(int remoteId, CancellationToken ct = default) =>
            ExecuteAsync(() => _api.DeleteTokenAsync(remoteId, ct), ct);

        public Task<ServiceResult> ClearApprovedTokensAsync(CancellationToken ct = default) =>
            ExecuteAsync(() => _api.ClearApprovedTokensAsync(ct), ct);

        public Task<ServiceResult> UpdateAdminPasswordAsync(int adminId, string newPassword, CancellationToken ct = default) =>
            ExecuteAsync(() => _api.UpdateAdminPasswordAsync(adminId, new UpdatePasswordRequestDto { Password = newPassword }, ct), ct);

        // ---- plumbing --------------------------------------------------------------------

        private async Task<ServiceResult> ExecuteAsync(Func<Task<ApiResponse<ApiEnvelopeDto>>> call, CancellationToken ct)
        {
            var result = await ExecuteAsync(call, dto => dto, ct);
            return result.IsSuccess ? ServiceResult.Ok(result.Message) : ServiceResult.Fail(result.Failure, result.Message);
        }

        /// <param name="map">Returns null when the (successful) response lacks the payload the app needs.</param>
        private async Task<ServiceResult<TApp>> ExecuteAsync<TDto, TApp>(
            Func<Task<ApiResponse<TDto>>> call,
            Func<TDto, TApp?> map,
            CancellationToken ct)
            where TDto : ApiEnvelopeDto
            where TApp : class
        {
            if (!_connectivity.IsOnline)
            {
                return ServiceResult<TApp>.Fail(FailureKind.Offline, OfflineMessage);
            }

            try
            {
                using var response = await call().ConfigureAwait(false);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    if (!response.Content.Success)
                    {
                        return ServiceResult<TApp>.Fail(FailureKind.Validation, response.Content.Message ?? GenericMessage);
                    }

                    var mapped = map(response.Content);
                    return mapped == null
                        ? ServiceResult<TApp>.Fail(FailureKind.Unknown, GenericMessage)
                        : ServiceResult<TApp>.Ok(mapped, response.Content.Message);
                }

                return Failure<TApp>(response);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                // HttpClient timeout
                return ServiceResult<TApp>.Fail(FailureKind.Network, NetworkMessage);
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API network error: {ex.GetType().Name}");
                return ServiceResult<TApp>.Fail(FailureKind.Network, NetworkMessage);
            }
            catch (ApiException ex)
            {
                System.Diagnostics.Debug.WriteLine($"API error: {(int)ex.StatusCode}");
                return ServiceResult<TApp>.Fail(FailureKind.Unknown, GenericMessage);
            }
        }

        private static ServiceResult<T> Failure<T>(IApiResponse response)
        {
            var status = response.StatusCode;
            System.Diagnostics.Debug.WriteLine($"API returned HTTP {(int?)status}");

            return status switch
            {
                HttpStatusCode.Unauthorized => ServiceResult<T>.Fail(FailureKind.Unauthorized, ReadMessage(response) ?? SessionExpiredMessage),
                HttpStatusCode.Forbidden => ServiceResult<T>.Fail(FailureKind.Forbidden, ForbiddenMessage),
                HttpStatusCode.UnprocessableEntity => ServiceResult<T>.Fail(FailureKind.Validation, ReadMessage(response) ?? GenericMessage),
                HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests => ServiceResult<T>.Fail(FailureKind.Server, GenericMessage),
                >= HttpStatusCode.InternalServerError => ServiceResult<T>.Fail(FailureKind.Server, GenericMessage),
                _ => ServiceResult<T>.Fail(FailureKind.Unknown, ReadMessage(response) ?? GenericMessage)
            };
        }

        private static string? ReadMessage(IApiResponse response)
        {
            var body = (response.Error as ApiException)?.Content;
            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorDto>(body);
                if (!string.IsNullOrWhiteSpace(error?.Message))
                    return error.Message;

                // Prefer the first field-level validation message when no top-level message is present.
                return error?.Errors?.Values.SelectMany(v => v).FirstOrDefault();
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
