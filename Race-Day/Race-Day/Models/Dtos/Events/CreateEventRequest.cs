using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Events;

/// <summary>Request body for creating a new event (Organiser only).</summary>
public class CreateEventRequest
{
    /// <summary>Public name of the event.</summary>
    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>Date the event takes place.</summary>
    [Required(ErrorMessage = "Event date is required.")]
    public DateOnly EventDate { get; set; }

    /// <summary>Location of the event.</summary>
    [Required(ErrorMessage = "Location is required.")]
    [StringLength(150)]
    public string Location { get; set; } = string.Empty;

    /// <summary>Optional route information.</summary>
    [StringLength(255)]
    public string? RouteInfo { get; set; }

    /// <summary>Optional long description.</summary>
    public string? Description { get; set; }
}
