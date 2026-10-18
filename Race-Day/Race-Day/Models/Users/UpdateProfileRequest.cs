using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Users;

/// <summary>Request body for updating the logged-in user's own profile.</summary>
public class UpdateProfileRequest
{
    /// <summary>New full name.</summary>
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>New contact number (optional).</summary>
    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}
