using Fuxion.Ali.Contracts.Auth.Login;
using Fuxion.Ali.Contracts.Common;
using System.Security.Claims;

namespace Fuxion.Ali.PWA.Services
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetUserNameAsync();
        Task<IEnumerable<string>> GetRolesAsync();
        Task<bool> IsInRoleAsync(string role);
        Task<IEnumerable<Claim>> GetClaimsAsync();
    }
}
