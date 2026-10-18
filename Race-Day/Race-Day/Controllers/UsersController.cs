using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Data;
using Race_Day.Models.Dtos.Users;
using Race_Day.Mapping;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// User profile endpoints: read and update the logged-in user's own profile, change the
/// password, and view another user's limited public profile.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UsersController> _logger;

    /// <summary>Creates a new users controller.</summary>
    public UsersController(
        RaceDayDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        ILogger<UsersController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Gets the logged-in user's own full profile.</summary>
    /// <response code="200">The profile of the authenticated user.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The authenticated user could not be found.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.UserID == _currentUser.UserId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.ToProfileDto());
    }

    /// <summary>Updates the logged-in user's own profile (full name and phone number).</summary>
    /// <param name="request">The new profile values.</param>
    /// <response code="200">The updated profile.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The authenticated user could not be found.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.UserID == _currentUser.UserId);

        if (user is null)
        {
            return NotFound();
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        await _context.SaveChangesAsync();

        return Ok(user.ToProfileDto());
    }

    /// <summary>Changes the logged-in user's password after verifying the current one.</summary>
    /// <param name="request">The current and new passwords.</param>
    /// <response code="204">The password was changed.</response>
    /// <response code="400">The current password was incorrect.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.UserID == _currentUser.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Current password incorrect",
                detail: "The supplied current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} changed their password", user.UserID);

        return NoContent();
    }

    /// <summary>Gets another user's limited public profile.</summary>
    /// <param name="id">The identifier of the user to look up.</param>
    /// <response code="200">The limited public profile.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No user with the supplied id exists.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.UserID == id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.ToPublicProfileDto());
    }
}
