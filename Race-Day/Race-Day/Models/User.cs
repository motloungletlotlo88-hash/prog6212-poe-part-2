namespace Race_Day.Models;

/// <summary>
/// An authenticated account. A user is either an <c>Organiser</c> (creates and manages
/// events) or a <c>Participant</c> (enrols in categories and records results).
/// </summary>
public class User
{
    /// <summary>Primary key.</summary>
    public int UserID { get; set; }

    /// <summary>Foreign key to <see cref="Role"/>.</summary>
    public int RoleID { get; set; }

    /// <summary>Navigation property to the user's role.</summary>
    public Role? Role { get; set; }

    /// <summary>The user's full display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Unique email address used to log in.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Salted, hashed password. The plaintext password is never stored.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Optional contact number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>UTC timestamp of when the account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Events this user organises (only populated for Organisers).</summary>
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

    /// <summary>Enrolments belonging to this user (only populated for Participants).</summary>
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
