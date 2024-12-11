using Presentation.Models;
using Refit;

namespace Presentation.Services;

public interface IAuthService
{
    [Post("/api/auth/login")]
    Task<ApiResponse<LoginResponseDto>> LoginAsync([Body] LoginRequestDto request);

    [Post("/api/auth/refresh-token")]
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync([Body] RefreshTokenRequestDto request);

    [Post("/api/auth/revoke")]
    Task<ApiResponse<bool>> RevokeRefreshTokenAsync(Guid userId);
}