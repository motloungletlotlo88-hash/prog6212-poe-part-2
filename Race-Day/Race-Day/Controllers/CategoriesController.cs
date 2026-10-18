using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Common;
using Race_Day.Data;
using Race_Day.Models.Dtos.Categories;
using Race_Day.Models;
using Race_Day.Mapping;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// Category endpoints. Listing categories is public; creating, updating and deleting them
/// requires the <c>Organiser</c> role and ownership of the parent event.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CategoriesController> _logger;

    /// <summary>Creates a new categories controller.</summary>
    public CategoriesController(
        RaceDayDbContext context,
        ICurrentUserService currentUser,
        ILogger<CategoriesController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Lists all categories that belong to an event. This endpoint is public.</summary>
    /// <param name="eventId">The parent event identifier.</param>
    /// <response code="200">An array of categories for the event.</response>
    /// <response code="404">No event with the supplied id exists.</response>
    [HttpGet("events/{eventId:int}/categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesForEvent(int eventId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
        if (!eventExists)
        {
            return NotFound();
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.DistanceKM)
            .ToListAsync();

        return Ok(categories.Select(c => c.ToDto()));
    }

    /// <summary>Adds a new category to an event owned by the logged-in Organiser.</summary>
    /// <param name="eventId">The parent event identifier.</param>
    /// <param name="request">The category details.</param>
    /// <response code="201">The category was created.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No event with the supplied id exists.</response>
    [HttpPost("events/{eventId:int}/categories")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
    {
        var parent = await _context.Events.SingleOrDefaultAsync(e => e.EventID == eventId);
        if (parent is null)
        {
            return NotFound();
        }

        if (parent.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        var entity = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKM = request.DistanceKm,
            MaxParticipants = request.MaxParticipants,
            EntryFee = request.EntryFee
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organiser {OrganiserId} added category {CategoryId} to event {EventId}",
            _currentUser.UserId, entity.CategoryID, eventId);

        return StatusCode(StatusCodes.Status201Created, entity.ToDto());
    }

    /// <summary>Updates a category whose parent event is owned by the logged-in Organiser.</summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="request">The new category details.</param>
    /// <response code="200">The updated category.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No category with the supplied id exists.</response>
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var entity = await _context.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c => c.CategoryID == id);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Event is null || entity.Event.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        entity.CategoryName = request.CategoryName.Trim();
        entity.DistanceKM = request.DistanceKm;
        entity.MaxParticipants = request.MaxParticipants;
        entity.EntryFee = request.EntryFee;

        await _context.SaveChangesAsync();

        return Ok(entity.ToDto());
    }

    /// <summary>Deletes a category whose parent event is owned by the logged-in Organiser.</summary>
    /// <param name="id">The category identifier.</param>
    /// <response code="204">The category was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No category with the supplied id exists.</response>
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var entity = await _context.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c => c.CategoryID == id);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Event is null || entity.Event.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
