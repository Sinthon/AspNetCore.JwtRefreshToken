using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api;

public class JwtBearerOptionsSetup(IConfiguration _config) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);
        var signingKey = new SymmetricSecurityKey(key);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero // Optional: Adjusts clock skew tolerance for token expiration
        };
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);
}
