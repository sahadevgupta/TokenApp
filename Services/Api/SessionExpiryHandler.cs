using System.Net;
using TokenApp.Services.Auth;

namespace TokenApp.Services.Api
{
    /// <summary>
    /// Implements the documented 401 rule: token expired/missing → clear the stored session and
    /// let the app send the user back to Login. Only reacts to requests that carried a bearer
    /// token, so a wrong password on /admin/login is not mistaken for an expired session.
    /// The API documents no refresh endpoint, so there is nothing to refresh and retry.
    /// </summary>
    public class SessionExpiryHandler : DelegatingHandler
    {
        private readonly ISessionService _session;

        public SessionExpiryHandler(ISessionService session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized && request.Headers.Authorization != null)
            {
                await _session.ExpireAsync().ConfigureAwait(false);
            }

            return response;
        }
    }
}
