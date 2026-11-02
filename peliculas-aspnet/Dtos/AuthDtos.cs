namespace PeliculasAspnet.Dtos;

public class RegisterDto
{
    public string? Nombre { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }
}

public class LoginDto
{
    public string? Email { get; set; }

    public string? Password { get; set; }
}

public class AuthUserDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public AuthUserDto Usuario { get; set; } = new();
}
