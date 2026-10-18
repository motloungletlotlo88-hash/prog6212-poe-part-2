namespace Race_Day.Models.Dtos.Users;

/// <summary>The full profile of the currently authenticated user.</summary>
public class UserProfileDto
{
    /// <summary>User identifier.</summary>
    public int UserId { get; set; }

    /// <summary>Full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Contact number, if provided.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Role name.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>UTC timestamp of account creation.</summary>
    public DateTime CreatedAt { get; set; }
}
