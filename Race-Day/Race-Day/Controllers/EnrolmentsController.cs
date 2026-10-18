using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Common;
using Race_Day.Data;
using Race_Day.Models.Dtos.Enrolments;
using Race_Day.Models;
using Race_Day.Mapping;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// Enrolment endpoints. Participants enrol in categories, list and cancel their own
/// enrolments; Organisers list the participants enrolled in their categories.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EnrolmentsController> _logger;

    /// <summary>Creates a new enrolments controller.</summary>
    public EnrolmentsController(
        RaceDayDbContext context,
        ICurrentUserService currentUser,
        ILogger<EnrolmentsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Enrols the logged-in Participant into a category.</summary>
    /// <param name="categoryId">The category to enrol in.</param>
    /// <response code="201">The enrolment was created.</response>
    /// <response code="400">The category is full or the caller is already enrolled.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not a Participant.</response>
    /// <response code="404">No category with the supplied id exists.</response>
    [HttpPost("categories/{categoryId:int}/enrolments")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrolmentDto>> Enrol(int categoryId)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.CategoryID == categoryId);
        if (category is null)
        {
            return NotFound();
        }

        var participantId = _currentUser.UserId!.Value;

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e => e.ParticipantID == participantId && e.CategoryID == categoryId);

        if (alreadyEnrolled)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Already enrolled",
                detail: "You are already enrolled in this category.");
        }

        var currentCount = await _context.Enrolments
            .CountAsync(e => e.CategoryID == categoryId && e.Status != "Cancelled");

        if (currentCount >= category.MaxParticipants)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Category full",
                detail: "This category has reached its maximum number of participants.");
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            CategoryID = categoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Participant {ParticipantId} enrolled in category {CategoryId}",
            participantId, categoryId);

        return StatusCode(StatusCodes.Status201Created, enrolment.ToDto());
    }

    /// <summary>Lists the logged-in Participant's own enrolments.</summary>
    /// <response code="200">An array of the participant's enrolments.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not a Participant.</response>
    [HttpGet("users/me/enrolments")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<MyEnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<MyEnrolmentDto>>> GetMyEnrolments()
    {
        var participantId = _currentUser.UserId!.Value;

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Where(e => e.ParticipantID == participantId)
            .OrderBy(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments.Select(e => e.ToMyEnrolmentDto()));
    }

    /// <summary>Lists the participants enrolled in a category owned by the logged-in Organiser.</summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <response code="200">An array of enrolments for the category.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No category with the supplied id exists.</response>
    [HttpGet("categories/{categoryId:int}/enrolments")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<CategoryEnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CategoryEnrolmentDto>>> GetCategoryEnrolments(int categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c => c.CategoryID == categoryId);

        if (category is null)
        {
            return NotFound();
        }

        if (category.Event is null || category.Event.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Participant)
            .Where(e => e.CategoryID == categoryId)
            .OrderBy(e => e.EnrolmentDate)
            .ToListAsync();

        return Ok(enrolments.Select(e => e.ToCategoryEnrolmentDto()));
    }

    /// <summary>Cancels one of the logged-in Participant's own enrolments.</summary>
    /// <param name="id">The enrolment identifier.</param>
    /// <response code="204">The enrolment was cancelled.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not a Participant or does not own the enrolment.</response>
    /// <response code="404">No enrolment with the supplied id exists.</response>
    [HttpDelete("enrolments/{id:int}")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var enrolment = await _context.Enrolments.SingleOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment is null)
        {
            return NotFound();
        }

        if (enrolment.ParticipantID != _currentUser.UserId)
        {
            return Forbid();
        }

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Participant {ParticipantId} cancelled enrolment {EnrolmentId}",
            _currentUser.UserId, id);

        return NoContent();
    }
}
