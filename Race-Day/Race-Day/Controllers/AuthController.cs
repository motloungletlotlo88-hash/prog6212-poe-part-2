using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Race_Day.Common;
using Race_Day.Data;
using Race_Day.Models.Dtos.Auth;
using Race_Day.Models;
using Race_Day.Services;

namespace Race_Day.Controllers;

/// <summary>
/// Authentication endpoints: register a new account, log in to obtain a bearer token, and log
/// out to revoke the current token.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITokenBlacklist _tokenBlacklist;
    private readonly ILogger<AuthController> _logger;

    /// <summary>Creates a new authentication controller.</summary>
    public AuthController(
        RaceDayDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITokenBlacklist tokenBlacklist,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _tokenBlacklist = tokenBlacklist;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user as either an Organiser or a Participant.
    /// </summary>
    /// <remarks>
    /// The password is hashed before storage and is never persisted in plaintext. On success a
    /// signed JWT is returned so the caller is immediately authenticated.
    /// </remarks>
    /// <param name="request">The registration details (full name, email, password, phone number and role).</param>
    /// <response code="201">The account was created and a token was issued.</response>
    /// <response code="400">The request body failed validation or the role was not recognised.</response>
    /// <response code="409">An account with the supplied email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!RoleNames.IsValid(request.Role))
        {
            return BadRequest(Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid role",
                detail: $"Role must be one of: {string.Join(", ", RoleNames.All)}."));
        }

        var roleName = RoleNames.Canonical(request.Role);
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _context.Users.AnyAsync(u => u.Email.ToLower() == email);
        if (emailTaken)
        {
            return Conflict(Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Email already registered",
                detail: "An account with this email address already exists."));
        }

        var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == roleName);
        if (role is null)
        {
            // Should never happen once roles are seeded, but fail loudly rather than silently.
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Role not configured",
                detail: $"The '{roleName}' role has not been seeded.");
        }

        var user = new User
        {
            RoleID = role.RoleID,
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered new {Role} {Email}", roleName, email);

        var (token, expiresAt) = _tokenService.CreateToken(user, roleName);

        var response = new AuthResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            Token = token,
            ExpiresAt = expiresAt
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Authenticates a user and issues a bearer token.
    /// </summary>
    /// <param name="request">The user's email and password.</param>
    /// <response code="200">Credentials were valid and a token was issued.</response>
    /// <response code="401">The email or password was incorrect.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Deliberately identical response for "unknown email" and "wrong password".
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials",
                detail: "The email address or password is incorrect.");
        }

        var roleName = user.Role?.RoleName ?? RoleNames.Participant;
        var (token, expiresAt) = _tokenService.CreateToken(user, roleName);

        _logger.LogInformation("User {Email} logged in", email);

        return Ok(new AuthResponse
        {
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// Logs out the current user by revoking the bearer token supplied in the request.
    /// </summary>
    /// <response code="204">The token was revoked (or was already invalid).</response>
    /// <response code="401">No valid token was supplied.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expiryRaw = User.FindFirst("exp")?.Value;

        if (!string.IsNullOrWhiteSpace(jti))
        {
            var expiresAt = DateTime.UtcNow.AddHours(1);
            if (long.TryParse(expiryRaw, out var unixSeconds))
            {
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
            }

            _tokenBlacklist.Revoke(jti, expiresAt);
        }

        return NoContent();
    }
}
