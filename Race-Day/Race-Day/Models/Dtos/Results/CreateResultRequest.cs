using System.ComponentModel.DataAnnotations;

namespace Race_Day.Models.Dtos.Results;

/// <summary>Request body for capturing a result against an enrolment (Organiser only).</summary>
public class CreateResultRequest
{
    /// <summary>Finish time in <c>hh:mm:ss</c> format (optional).</summary>
    public TimeSpan? FinishTime { get; set; }

    /// <summary>Finishing position (optional).</summary>
    [Range(1, 100000, ErrorMessage = "Position must be a positive number.")]
    public int? Position { get; set; }

    /// <summary>Status of the result, for example <c>Finished</c> or <c>DNF</c>.</summary>
    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20)]
    public string Status { get; set; } = "Finished";
}
