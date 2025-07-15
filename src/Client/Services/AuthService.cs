using System.Net.Http.Json;
using TimeStock.Shared.Dtos;

namespace Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly AuthTokenStorageService _tokenStorage;
        private readonly CustomAuthStateProvider _authState;

        public AuthService(HttpClient http, AuthTokenStorageService tokenStorage, CustomAuthStateProvider authState)
        {
            _http = http;
            _tokenStorage = tokenStorage;
            _authState = authState;
        }

        public async Task<bool> LoginAsync(LoginDto login)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", login);

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (data is null)
                return false;

            // Stockage du token et des données utilisateur
            await _tokenStorage.StoreTokenAsync(data.Token);
            await _tokenStorage.StoreUserDataAsync(data);

            // Notifier Blazor que l’utilisateur est maintenant connecté
            _authState.NotifyUserAuthentication();

            return true;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _tokenStorage.GetTokenAsync();
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var token = await _tokenStorage.GetTokenAsync();
            return !string.IsNullOrWhiteSpace(token);
        }

        public async Task LogoutAsync()
        {
            await _tokenStorage.ClearAsync(); // supprime tout (token + userData)

            // Notifie Blazor que l’utilisateur est maintenant déconnecté
            _authState.NotifyUserLogout();
        }

        public async Task<bool> RegisterAndLoginAsync(UserDto newUser)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/register", newUser);
            if (!resp.IsSuccessStatusCode) return false;

            var data = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (data is null) return false;

            await _tokenStorage.StoreTokenAsync(data.Token);
            await _tokenStorage.StoreUserDataAsync(data);
            _authState.NotifyUserAuthentication();
            return true;
        }

        public async Task<MeResponseDto?> GetCurrentUserAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<MeResponseDto>("api/auth/me");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await LogoutAsync();
                return null;
            }
        }

    }
}