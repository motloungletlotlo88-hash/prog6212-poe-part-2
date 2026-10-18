namespace Race_Day.Models.Dtos.Events;

/// <summary>Public representation of an event.</summary>
public class EventDto
{
    /// <summary>Event identifier.</summary>
    public int EventId { get; set; }

    /// <summary>Identifier of the organising user.</summary>
    public int OrganiserId { get; set; }

    /// <summary>Public name of the event.</summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>Date the event takes place.</summary>
    public DateOnly EventDate { get; set; }

    /// <summary>Location of the event.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Optional route information.</summary>
    public string? RouteInfo { get; set; }

    /// <summary>Optional long description.</summary>
    public string? Description { get; set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; set; }
}
