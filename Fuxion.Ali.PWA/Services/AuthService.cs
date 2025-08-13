using Blazored.LocalStorage;
using Fuxion.Ali.Contracts.Auth.Login;
using Fuxion.Ali.Contracts.Common;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Fuxion.Ali.PWA.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        private const string AUTH_TOKEN = "authToken";

        public AuthService(ILogger<AuthService> logger, HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(AUTH_TOKEN);
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", request);
                var result = await response.Content.ReadFromJsonAsync<Result<LoginResponse>>();

                if (result?.IsSuccess == true && result.Data != null)
                {
                    await _localStorage.SetItemAsync(AUTH_TOKEN, result.Data.Token);
                    if (_authStateProvider is AuthStateProvider authProvider)
                        authProvider.NotifyUserAuthentication(result.Data.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Data.Token);
                }

                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Result<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = "Ocurrió un error inesperado."
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(AUTH_TOKEN);
            if (_authStateProvider is AuthStateProvider customProvider)
                customProvider.NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated ?? false;
        }

        public async Task<string?> GetUserNameAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.Name;
        }

        public async Task<IEnumerable<string>> GetRolesAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            var roles = await GetRolesAsync();
            return roles.Contains(role);
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Claims;
        }
    }
}
