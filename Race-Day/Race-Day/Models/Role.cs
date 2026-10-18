namespace Race_Day.Models;

/// <summary>
/// A security role that a user can hold. The system ships with exactly two roles:
/// <c>Organiser</c> and <c>Participant</c>.
/// </summary>
public class Role
{
    /// <summary>Primary key.</summary>
    public int RoleID { get; set; }

    /// <summary>Unique, human-readable role name.</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Navigation property: users that belong to this role.</summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}
