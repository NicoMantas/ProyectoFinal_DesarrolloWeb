using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasAspnet.Dtos;
using PeliculasAspnet.Services;

namespace PeliculasAspnet.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var response = await _auth.Register(dto);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var response = await _auth.SignIn(dto.Email ?? string.Empty, dto.Password ?? string.Empty);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var authorization = Request.Headers.Authorization.ToString();
        var token = authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization["Bearer ".Length..]
            : string.Empty;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var subValue = jwt.Payload.TryGetValue("sub", out var sub) ? sub?.ToString() : null;

        return Ok(new
        {
            sub = int.TryParse(subValue, out var id) ? id : subValue as object,
            email = jwt.Payload.TryGetValue("email", out var email) ? email : null,
            nombre = jwt.Payload.TryGetValue("nombre", out var nombre) ? nombre : null,
            iat = jwt.Payload.TryGetValue("iat", out var iat) ? iat : null,
            exp = jwt.Payload.TryGetValue("exp", out var exp) ? exp : null,
        });
    }
}
