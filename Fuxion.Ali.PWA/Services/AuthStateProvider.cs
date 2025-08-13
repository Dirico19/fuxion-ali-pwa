using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fuxion.Ali.PWA.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        private const string AUTH_TOKEN = "authToken";

        public AuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(AUTH_TOKEN);

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var user = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));

            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
