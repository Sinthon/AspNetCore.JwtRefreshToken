using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Features.Users;

public class RevokeRefreshToken(AuthContext _dbContext, IHttpContextAccessor _httpContext) : Endpoint<RevokeRefreshToken.Req>
{
    public override void Configure() => Post("/api/auth/revoke");
    public record Req(Guid UserId);

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        if (req.UserId != GetCurrentUserId())
            ThrowError("You are not do this.");

        await _dbContext.RefreshTokens.Where(r => r.UserId == req.UserId)
            .ExecuteDeleteAsync(ct);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = _httpContext.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid nameIdentifier) ?
            nameIdentifier : null;
    }
}
