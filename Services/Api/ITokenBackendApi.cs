using Refit;
using TokenApp.Models.Request;
using TokenApp.Models.Response;

namespace TokenApp.Services.Api
{
    /// <summary>
    /// Refit definition of the Mandir Token REST API (see "Mandir Token API Documentation").
    /// The bearer token is added to every request by <see cref="AuthorizationHandler"/>.
    /// Only endpoints whose response contract is documented are declared here.
    /// </summary>
    [Headers("Accept: application/json")]
    public interface ITokenBackendApi
    {
        [Post("/admin/login")]
        Task<ApiResponse<LoginResponseDto>> LoginAsync([Body] LoginRequestDto request, CancellationToken cancellationToken = default);

        [Get("/admin/dashboard")]
        Task<ApiResponse<DashboardResponseDto>> GetDashboardAsync(CancellationToken cancellationToken = default);

        [Post("/admin/generate-print-token")]
        Task<ApiResponse<TokenDataResponseDto>> GenerateAndPrintTokenAsync([Body] GenerateTokenRequestDto request, CancellationToken cancellationToken = default);

        [Post("/admin/approve/{id}")]
        Task<ApiResponse<ApiEnvelopeDto>> ApproveTokenAsync(int id, CancellationToken cancellationToken = default);

        [Post("/admin/reject/{id}")]
        Task<ApiResponse<ApiEnvelopeDto>> RejectTokenAsync(int id, [Body] RejectTokenRequestDto request, CancellationToken cancellationToken = default);

        [Delete("/admin/delete/{id}")]
        Task<ApiResponse<ApiEnvelopeDto>> DeleteTokenAsync(int id, CancellationToken cancellationToken = default);

        [Post("/admin/tokens/clear")]
        Task<ApiResponse<ApiEnvelopeDto>> ClearApprovedTokensAsync(CancellationToken cancellationToken = default);

        [Post("/admin/update-password/{id}")]
        Task<ApiResponse<ApiEnvelopeDto>> UpdateAdminPasswordAsync(int id, [Body] UpdatePasswordRequestDto request, CancellationToken cancellationToken = default);

        // The documentation lists no Authorization requirement for the two user endpoints below
        // (the handler still sends the token if a session exists; the server ignores it).
        [Post("/token/apply")]
        Task<ApiResponse<TokenDataResponseDto>> ApplyTokenAsync([Body] ApplyTokenRequestDto request, CancellationToken cancellationToken = default);

        [Get("/token/print/{id}")]
        Task<ApiResponse<PrintTokenResponseDto>> GetPrintTokenAsync(int id, CancellationToken cancellationToken = default);
    }
}
