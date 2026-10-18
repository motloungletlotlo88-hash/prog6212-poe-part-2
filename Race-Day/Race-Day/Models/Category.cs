namespace Race_Day.Models;

/// <summary>
/// A specific racing category within an <see cref="Event"/> (for example the 42 km
/// marathon or the 55 km cycle). Participants enrol into a category.
/// </summary>
public class Category
{
    /// <summary>Primary key.</summary>
    public int CategoryID { get; set; }

    /// <summary>Foreign key to the parent <see cref="Event"/>.</summary>
    public int EventID { get; set; }

    /// <summary>Navigation property to the parent event.</summary>
    public Event? Event { get; set; }

    /// <summary>Display name of the category.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Distance of the category in kilometres.</summary>
    public decimal DistanceKM { get; set; }

    /// <summary>Maximum number of participants allowed in this category.</summary>
    public int MaxParticipants { get; set; } = 100;

    /// <summary>Entry fee charged for the category.</summary>
    public decimal EntryFee { get; set; }

    /// <summary>Enrolments recorded against this category.</summary>
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
