namespace Race_Day.Models;

/// <summary>
/// A sporting event (for example a marathon or a cycle tour) created and owned by a
/// single Organiser. An event contains one or more <see cref="Category"/> entries.
/// </summary>
public class Event
{
    /// <summary>Primary key.</summary>
    public int EventID { get; set; }

    /// <summary>Foreign key to the organising <see cref="User"/>.</summary>
    public int OrganiserID { get; set; }

    /// <summary>Navigation property to the owning organiser.</summary>
    public User? Organiser { get; set; }

    /// <summary>Public name of the event.</summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>Date on which the event takes place.</summary>
    public DateOnly EventDate { get; set; }

    /// <summary>Where the event is held.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Optional description of the route.</summary>
    public string? RouteInfo { get; set; }

    /// <summary>Optional long-form description of the event.</summary>
    public string? Description { get; set; }

    /// <summary>UTC timestamp of when the event was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Racing categories that belong to this event.</summary>
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
