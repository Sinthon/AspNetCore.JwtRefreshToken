using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api;

public class JwtProvider(IConfiguration _config)
{
    public string GenerateToken(IReadOnlyList<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8
            .GetBytes(_config["Jwt:SecretKey"]!);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _config["Jwt:Audience"],
            Issuer = _config["Jwt:Issuer"],
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:TokenExpiryMinutes"]!)),
            SigningCredentials = credentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        Span<byte> randomNumber = stackalloc byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
