using System.Security.Cryptography;
using System.Text;

namespace PeliculasAspnet.Services;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var salt = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var hash = HashWithSalt(password, salt);
        return $"{salt}:{hash}";
    }

    public static bool Verify(string password, string storedPassword)
    {
        var separatorIndex = storedPassword.IndexOf(':');
        if (separatorIndex < 0)
        {
            return false;
        }

        var salt = storedPassword[..separatorIndex];
        var storedHash = storedPassword[(separatorIndex + 1)..];
        var candidateHash = HashWithSalt(password, salt);
        return candidateHash == storedHash;
    }

    private static string HashWithSalt(string password, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
