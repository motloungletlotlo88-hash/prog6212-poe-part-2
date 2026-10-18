using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Common;
using Race_Day.Data;
using Race_Day.Models.Dtos.Events;
using Race_Day.Models;
using Race_Day.Mapping;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// Event endpoints. Browsing is public; creating, updating and deleting require the
/// <c>Organiser</c> role and are restricted to events the caller owns.
/// </summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EventsController> _logger;

    /// <summary>Creates a new events controller.</summary>
    public EventsController(
        RaceDayDbContext context,
        ICurrentUserService currentUser,
        ILogger<EventsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Lists all upcoming events. This endpoint is public.</summary>
    /// <response code="200">An array of events (empty when there are none).</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(events.Select(e => e.ToDto()));
    }

    /// <summary>Gets the full details of a single event, including route information.</summary>
    /// <param name="id">The event identifier.</param>
    /// <response code="200">The requested event.</response>
    /// <response code="404">No event with the supplied id exists.</response>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var entity = await _context.Events.AsNoTracking().SingleOrDefaultAsync(e => e.EventID == id);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(entity.ToDto());
    }

    /// <summary>Creates a new event owned by the logged-in Organiser.</summary>
    /// <param name="request">The event details.</param>
    /// <response code="201">The event was created.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var entity = new Event
        {
            OrganiserID = _currentUser.UserId!.Value,
            EventName = request.EventName.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            RouteInfo = string.IsNullOrWhiteSpace(request.RouteInfo) ? null : request.RouteInfo.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organiser {OrganiserId} created event {EventId}", entity.OrganiserID, entity.EventID);

        return CreatedAtAction(nameof(GetEvent), new { id = entity.EventID }, entity.ToDto());
    }

    /// <summary>Updates an event owned by the logged-in Organiser.</summary>
    /// <param name="id">The event identifier.</param>
    /// <param name="request">The new event details.</param>
    /// <response code="200">The updated event.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No event with the supplied id exists.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var entity = await _context.Events.SingleOrDefaultAsync(e => e.EventID == id);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        entity.EventName = request.EventName.Trim();
        entity.EventDate = request.EventDate;
        entity.Location = request.Location.Trim();
        entity.RouteInfo = string.IsNullOrWhiteSpace(request.RouteInfo) ? null : request.RouteInfo.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _context.SaveChangesAsync();

        return Ok(entity.ToDto());
    }

    /// <summary>Deletes an event owned by the logged-in Organiser.</summary>
    /// <param name="id">The event identifier.</param>
    /// <response code="204">The event was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No event with the supplied id exists.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var entity = await _context.Events.SingleOrDefaultAsync(e => e.EventID == id);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        _context.Events.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
