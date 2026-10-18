namespace Race_Day.Models.Dtos.Categories;

/// <summary>Public representation of a category belonging to an event.</summary>
public class CategoryDto
{
    /// <summary>Category identifier.</summary>
    public int CategoryId { get; set; }

    /// <summary>Identifier of the parent event.</summary>
    public int EventId { get; set; }

    /// <summary>Display name of the category.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Distance in kilometres.</summary>
    public decimal DistanceKm { get; set; }

    /// <summary>Maximum number of participants.</summary>
    public int MaxParticipants { get; set; }

    /// <summary>Entry fee.</summary>
    public decimal EntryFee { get; set; }
}
