using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Features.Users;

public class LoginUserWithRefreshToken(AuthContext _dbContext, JwtProvider _jwt) : Endpoint<LoginUserWithRefreshToken.Req, LoginUserWithRefreshToken.Res>
{
    public record Req(string RefreshToken);
    public record Res(string AccessToken, string RefreshToken);

    public override void Configure()
    {
        Post("/api/auth/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        RefreshToken? refreshToken =  _dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefault(x => x.Token == req.RefreshToken);

        if (refreshToken is null || refreshToken.ExpiresOn < DateTime.Now)
            ThrowError("Token has expired.");

        var accessToken = _jwt.GenerateToken([
            new Claim(ClaimTypes.NameIdentifier, refreshToken.User.Id.ToString()),
            new Claim(ClaimTypes.Name, refreshToken.User.Name)]);

        refreshToken.Token = _jwt.GenerateRefreshToken();
        refreshToken.ExpiresOn = DateTime.Now.AddDays(7);

        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new Res(accessToken, refreshToken.Token));
    }
}
