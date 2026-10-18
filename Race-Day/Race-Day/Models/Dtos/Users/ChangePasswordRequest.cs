using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Users;

/// <summary>Request body for changing the logged-in user's password.</summary>
public class ChangePasswordRequest
{
    /// <summary>The user's current password, verified before the change.</summary>
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>The desired new password.</summary>
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
