namespace Race_Day.Models.Dtos.Users;

/// <summary>The limited, publicly visible profile of another user.</summary>
public class PublicProfileDto
{
    /// <summary>User identifier.</summary>
    public int UserId { get; set; }

    /// <summary>Full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Role name.</summary>
    public string Role { get; set; } = string.Empty;
}
