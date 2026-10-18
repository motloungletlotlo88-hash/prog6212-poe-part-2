using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Common;
using Race_Day.Data;
using Race_Day.Models.Dtos.Results;
using Race_Day.Models;
using Race_Day.Mapping;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// Result endpoints. Organisers capture and update results for enrolments on their own
/// events; Participants view the result of an enrolment and their full performance history.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ResultsController> _logger;

    /// <summary>Creates a new results controller.</summary>
    public ResultsController(
        RaceDayDbContext context,
        ICurrentUserService currentUser,
        ILogger<ResultsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Captures a result for a participant's enrolment on an event owned by the caller.</summary>
    /// <param name="id">The enrolment identifier.</param>
    /// <param name="request">The result details.</param>
    /// <response code="201">The result was captured.</response>
    /// <response code="400">A result already exists for this enrolment.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No enrolment with the supplied id exists.</response>
    [HttpPost("enrolments/{id:int}/results")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultDto>> CaptureResult(int id, [FromBody] CreateResultRequest request)
    {
        var enrolment = await _context.Enrolments
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Result)
            .SingleOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment is null)
        {
            return NotFound();
        }

        if (enrolment.Category?.Event is null || enrolment.Category.Event.OrganiserID != _currentUser.UserId)
        {
            return Forbid();
        }

        if (enrolment.Result is not null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Result already exists",
                detail: "A result has already been captured for this enrolment.");
        }

        var result = new Result
        {
            EnrolmentID = id,
            FinishTime = request.FinishTime,
            Position = request.Position,
            Status = request.Status.Trim()
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Organiser {OrganiserId} captured result {ResultId} for enrolment {EnrolmentId}",
            _currentUser.UserId, result.ResultID, id);

        return StatusCode(StatusCodes.Status201Created, result.ToDto());
    }

    /// <summary>Updates a captured result on an event owned by the logged-in Organiser.</summary>
    /// <param name="id">The result identifier.</param>
    /// <param name="request">The new result details.</param>
    /// <response code="200">The updated result.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Organiser or does not own the event.</response>
    /// <response code="404">No result with the supplied id exists.</response>
    [HttpPut("results/{id:int}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultDto>> UpdateResult(int id, [FromBody] UpdateResultRequest request)
    {
        var result = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .SingleOrDefaultAsync(r => r.ResultID == id);

        if (result is null)
        {
            return NotFound();
        }

        var organiserId = result.Enrolment?.Category?.Event?.OrganiserID;
        if (organiserId != _currentUser.UserId)
        {
            return Forbid();
        }

        result.FinishTime = request.FinishTime;
        result.Position = request.Position;
        result.Status = request.Status.Trim();

        await _context.SaveChangesAsync();

        return Ok(result.ToDto());
    }

    /// <summary>Views the result for one of the logged-in Participant's own enrolments.</summary>
    /// <param name="id">The enrolment identifier.</param>
    /// <response code="200">The result for the enrolment.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not the participant or the event owner.</response>
    /// <response code="404">No enrolment or result exists.</response>
    [HttpGet("enrolments/{id:int}/results")]
    [Authorize(Roles = $"{RoleNames.Participant},{RoleNames.Organiser}")]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultDto>> GetResultForEnrolment(int id)
    {
        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Result)
            .SingleOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment is null || enrolment.Result is null)
        {
            return NotFound();
        }

        var isParticipant = enrolment.ParticipantID == _currentUser.UserId;
        var isEventOwner = enrolment.Category?.Event?.OrganiserID == _currentUser.UserId;

        if (!isParticipant && !isEventOwner)
        {
            return Forbid();
        }

        return Ok(enrolment.Result.ToDto());
    }

    /// <summary>Views the logged-in Participant's full personal performance history.</summary>
    /// <response code="200">An array of the participant's results.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not a Participant.</response>
    [HttpGet("users/me/results")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(IEnumerable<PerformanceHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PerformanceHistoryDto>>> GetMyPerformanceHistory()
    {
        var participantId = _currentUser.UserId!.Value;

        var results = await _context.Results
            .AsNoTracking()
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrolment!.ParticipantID == participantId)
            .ToListAsync();

        return Ok(results.Select(r => r.ToHistoryDto()));
    }
}
