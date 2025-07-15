using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TimeStock.Shared.Dtos;

namespace Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthTokenStorageService _storage;

        public CustomAuthStateProvider(AuthTokenStorageService storage)
        {
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _storage.GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var userData = await _storage.GetUserDataAsync();

            if (userData == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            Console.WriteLine("jusqu'ici tout va bien");
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, userData?.Email ?? ""),
                new Claim("account", userData?.AccountName ?? ""),
                new Claim("db", userData?.DatabaseName ?? "")
            }, "jwt");

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication()
        {
            Console.WriteLine("[AuthProvider] NotifyUserAuthentication() déclenché");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            var anon = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anon)));
        }
    }
}
