namespace Race_Day.Services;

/// <summary>
/// Tracks tokens that have been explicitly revoked (logged out). Because JWTs are stateless,
/// logout is implemented by recording the token's unique id until it naturally expires.
/// </summary>
public interface ITokenBlacklist
{
    /// <summary>Revokes a token by its unique id until the supplied expiry time.</summary>
    void Revoke(string jti, DateTime expiresAt);

    /// <summary>Returns <c>true</c> when the token id has been revoked and is still valid.</summary>
    bool IsRevoked(string jti);
}
