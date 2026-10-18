namespace Race_Day.Services;

/// <summary>Hashes and verifies user passwords. Plaintext passwords are never stored.</summary>
public interface IPasswordHasher
{
    /// <summary>Produces a salted, one-way hash of the supplied password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plaintext password against a previously produced hash.</summary>
    bool Verify(string password, string hash);
}
