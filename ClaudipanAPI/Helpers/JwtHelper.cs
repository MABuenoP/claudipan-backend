using ClaudipanAPI.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClaudipanAPI.Helpers;

/// <summary>
/// Helper para la generación de tokens JWT y refresh tokens.
/// </summary>
public static class JwtHelper
{
    public static string GenerateToken(Usuario usuario, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKeyString = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? jwtSettings["SecretKey"]
            ?? "Claudipan_SuperSecretKey_2026_Panaderia_Artesanal_PedroLeyva_SENA_ADSO_!@#$%";

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? jwtSettings["Issuer"]
            ?? "ClaudipanAPI";

        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? jwtSettings["Audience"]
            ?? "ClaudipanFrontend";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim("nameid", usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("email", usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "1440"));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}