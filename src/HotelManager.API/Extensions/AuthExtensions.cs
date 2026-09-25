using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HotelManager.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = configuration["Jwt:Key"];

        // Fail fast on missing/weak keys instead of crashing cryptically
        // (null) or signing weak tokens (short key). The committed fallback
        // is public, so it is rejected even though it passes the length check.
        // In production provide the key via the Jwt__Key environment variable.
        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32
            || signingKey == "PaintShopDefaultSecretKey_ChangeInProduction!")
            throw new InvalidOperationException("Jwt:Key must be set to a private 256-bit (32+ characters) key.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
