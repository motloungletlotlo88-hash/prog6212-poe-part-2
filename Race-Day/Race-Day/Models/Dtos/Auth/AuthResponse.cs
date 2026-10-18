namespace Race_Day.Models.Dtos.Auth;

/// <summary>Response returned after a successful registration or login.</summary>
public class AuthResponse
{
    /// <summary>The authenticated user's identifier.</summary>
    public int UserId { get; set; }

    /// <summary>The authenticated user's full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The authenticated user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The authenticated user's role (<c>Organiser</c> or <c>Participant</c>).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>The signed JWT bearer token used for subsequent requests.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>UTC time at which the token expires.</summary>
    public DateTime ExpiresAt { get; set; }
}
