using Race_Day.Models;

namespace Race_Day.Services;

/// <summary>Issues signed JWT bearer tokens that carry the user's identity and role.</summary>
public interface ITokenService
{
    /// <summary>Creates a token for the supplied user and role.</summary>
    /// <returns>The serialized token and its UTC expiry time.</returns>
    (string Token, DateTime ExpiresAt) CreateToken(User user, string roleName);
}
