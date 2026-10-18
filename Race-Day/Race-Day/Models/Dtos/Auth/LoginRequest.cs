using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Auth;

/// <summary>Request body for authenticating an existing user.</summary>
public class LoginRequest
{
    /// <summary>The registered email address.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>The account password.</summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
