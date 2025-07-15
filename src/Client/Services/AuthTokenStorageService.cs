using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;
using TimeStock.Shared.Dtos;

namespace Client.Services
{
    public class AuthTokenStorageService
    {
        private readonly IJSRuntime _js;
        private const string TokenKey = "auth_token";
        private const string UserDataKey = "user_data";

        public AuthTokenStorageService(IJSRuntime js)
        {
            _js = js;
        }

        // Stocke le token JWT
        public ValueTask StoreTokenAsync(string token)
        {
            return _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }

        // Récupère le token JWT
        public ValueTask<string?> GetTokenAsync()
        {
            return _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }

        // Supprime le token JWT
        public ValueTask RemoveTokenAsync()
        {
            return _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }

        // Stocke les infos utilisateur (email, accountName, etc.)
        public ValueTask StoreUserDataAsync(LoginResponseDto userData)
        {
            var json = JsonSerializer.Serialize(userData);
            return _js.InvokeVoidAsync("localStorage.setItem", UserDataKey, json);
        }

        // Récupère le JSON brut
        public ValueTask<string?> GetUserDataJsonAsync()
        {
            return _js.InvokeAsync<string?>("localStorage.getItem", UserDataKey);
        }

        // Désérialise en LoginResponseDto
        public async Task<LoginResponseDto?> GetUserDataAsync()
        {
            var json = await GetUserDataJsonAsync();
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<LoginResponseDto>(json);
        }

        // Supprime toutes les données
        public async Task ClearAsync()
        {
            await RemoveTokenAsync();
            await _js.InvokeVoidAsync("localStorage.removeItem", UserDataKey);
        }
    }
}
