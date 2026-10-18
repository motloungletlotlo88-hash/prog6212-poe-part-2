using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Auth;

/// <summary>Request body for registering a new Organiser or Participant account.</summary>
public class RegisterRequest
{
    /// <summary>The user's full name.</summary>
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>The user's unique email address.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>The chosen plaintext password (hashed before storage).</summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Optional contact number.</summary>
    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>Either <c>Organiser</c> or <c>Participant</c>.</summary>
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}
