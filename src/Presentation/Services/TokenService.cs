using Microsoft.JSInterop;
using Presentation.Models;

namespace Presentation.Services;

public class TokenService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IAuthService _authService;
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public TokenService(IJSRuntime jsRuntime, IAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
    }

    public async Task ClearTokensAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }

    public async Task<string?> RefreshAccessTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        try
        {
            var response = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            });

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                await SaveTokensAsync(
                    response.Content.AccessToken, 
                    response.Content.RefreshToken);
                return response.Content.AccessToken;
            }
        }
        catch
        {
            // Handle refresh token failure, log if necessary
        }

        return null;
    }
}
