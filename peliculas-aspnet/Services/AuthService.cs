using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PeliculasAspnet.Dtos;
using PeliculasAspnet.Exceptions;
using PeliculasAspnet.Models;

namespace PeliculasAspnet.Services;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto data);

    Task<AuthResponseDto> SignIn(string email, string password);
}

public class AuthService : IAuthService
{
    private readonly IUsersService _users;
    private readonly string _secret;

    public AuthService(IUsersService users, IConfiguration configuration)
    {
        _users = users;
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? configuration["Jwt:Secret"]
            ?? "miClaveSuperSecretaYCompleja123!";
    }

    public async Task<AuthResponseDto> Register(RegisterDto data)
    {
        var nombre = RequireText(data.Nombre);
        var email = RequireText(data.Email);
        var password = RequireText(data.Password);
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingUser = await _users.FindOneByEmail(normalizedEmail);

        if (existingUser is not null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "El usuario ya existe", "Unauthorized");
        }

        var user = await _users.Create(nombre.Trim(), normalizedEmail, PasswordHasher.Hash(password));
        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> SignIn(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _users.FindOneByEmail(normalizedEmail);

        if (user is null || !PasswordHasher.Verify(password, user.Password))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Credenciales inválidas", "Unauthorized");
        }

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = CreateToken(user),
            Usuario = new AuthUserDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
            },
        };
    }

    private string CreateToken(User user)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
            SecurityAlgorithms.HmacSha256);

        var issuedAt = DateTimeOffset.UtcNow;
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("nombre", user.Nombre),
                new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            ],
            expires: issuedAt.UtcDateTime.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string RequireText(string? value)
    {
        if (value is null)
        {
            throw new ApiException(StatusCodes.Status500InternalServerError, "Internal server error", "Internal Server Error");
        }

        return value;
    }
}
