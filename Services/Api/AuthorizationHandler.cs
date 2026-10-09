using System.Net.Http.Headers;
using TokenApp.Services.Auth;

namespace TokenApp.Services.Api
{
    /// <summary>
    /// Attaches the stored bearer token to every outgoing backend request, so individual Refit
    /// methods don't need any auth attribute. Requests made while signed out (e.g. /admin/login)
    /// go out without an Authorization header. Must be registered before
    /// <see cref="SessionExpiryHandler"/> so that handler can see the header it added.
    /// </summary>
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly ISessionService _session;

        public AuthorizationHandler(ISessionService session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                var token = await _session.GetTokenAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
