using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Presentation.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenService _tokenService;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public JwtAuthenticationStateProvider(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
                return new AuthenticationState(_anonymous);

            try
            {
                var claimsPrincipal = GetPrincipalFromToken(accessToken);
                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                // Attempt to refresh the token if expired
                var newAccessToken = await _tokenService.RefreshAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(newAccessToken))
                {
                    return await GetAuthenticationStateAsync(); // Retry with new token
                }

                await _tokenService.ClearTokensAsync();
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task MarkUserAsAuthenticated(string accessToken, string refreshToken)
        {
            await _tokenService.SaveTokensAsync(accessToken, refreshToken);

            var claimsPrincipal = GetPrincipalFromToken(accessToken);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _tokenService.ClearTokensAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }


        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
                throw new SecurityTokenException("Token has expired");

            // Map `role` claims to `ClaimTypes.Role`
            var claims = jwtToken.Claims.Select(claim =>
                claim.Type == "role"
                    ? new Claim(ClaimTypes.Role, claim.Value)
                    : claim);

            var identity = new ClaimsIdentity(claims, "jwt");

            return new ClaimsPrincipal(identity);
        }
    }
}

