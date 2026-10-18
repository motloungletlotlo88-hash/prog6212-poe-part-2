using System.Security.Cryptography;

namespace Race_Day.Services;

/// <summary>
/// PBKDF2 (RFC 2898) password hasher. Each password is hashed with a unique random salt and
/// a high iteration count, and verification uses a constant-time comparison to avoid timing
/// attacks. The stored value is <c>base64(salt).iterations.base64(hash)</c>.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;      // 128-bit salt
    private const int KeySize = 32;       // 256-bit derived key
    private const int Iterations = 100_000;
    private const char Delimiter = '.';

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            Delimiter,
            Convert.ToBase64String(salt),
            Iterations,
            Convert.ToBase64String(key));
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var parts = hash.Split(Delimiter);
        if (parts.Length != 3)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var iterations = int.Parse(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);
            return CryptographicOperations.FixedTimeEquals(key, attempt);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
