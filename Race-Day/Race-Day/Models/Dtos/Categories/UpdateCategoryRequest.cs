using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Categories;

/// <summary>Request body for updating a category owned by the logged-in Organiser.</summary>
public class UpdateCategoryRequest
{
    /// <summary>Display name of the category.</summary>
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Distance in kilometres.</summary>
    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    public decimal DistanceKm { get; set; }

    /// <summary>Maximum number of participants.</summary>
    [Range(1, 100000, ErrorMessage = "Max participants must be at least 1.")]
    public int MaxParticipants { get; set; } = 100;

    /// <summary>Entry fee.</summary>
    [Range(0, 1000000, ErrorMessage = "Entry fee cannot be negative.")]
    public decimal EntryFee { get; set; }
}
