using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Features.Users;

public sealed class LoginUser(AuthContext _dbContext, JwtProvider _jwt) : Endpoint<LoginUser.Req, LoginUser.Res>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public record Req(string Email, string Password);
    public record Res(string AccessToken, string RefreshToken);

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var user = await _dbContext.Users
            .Where(u => u.Email == req.Email && u.Password == req.Password)
            .FirstOrDefaultAsync();

        if (user == null)
            ThrowError("Invalid email or password.");

        var accessToken = _jwt.GenerateToken([
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name), 
            new Claim(ClaimTypes.Name, user.Name), 
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Distributor"),
        ]);

        var refreshToken = new RefreshToken()
        {
            User = user,
            Token = _jwt.GenerateRefreshToken(),
            ExpiresOn = DateTime.Now.AddDays(7)
        };

        _dbContext.Add(refreshToken);
        _dbContext.SaveChanges();

        await SendAsync(new Res(accessToken, 
            refreshToken.Token));
    }
}
