using System.Net.Http.Headers;


namespace Client.Services
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly AuthTokenStorageService _tokenStorage;

        public AuthMessageHandler(AuthTokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
