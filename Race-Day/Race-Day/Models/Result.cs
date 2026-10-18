namespace Race_Day.Models;

/// <summary>
/// The recorded outcome of a single <see cref="Enrolment"/>. Captured and updated by the
/// organiser that owns the underlying event.
/// </summary>
public class Result
{
    /// <summary>Primary key.</summary>
    public int ResultID { get; set; }

    /// <summary>Foreign key to the <see cref="Enrolment"/>. Unique: one result per enrolment.</summary>
    public int EnrolmentID { get; set; }

    /// <summary>Navigation property to the enrolment.</summary>
    public Enrolment? Enrolment { get; set; }

    /// <summary>Finish time of the participant (may be null for a did-not-finish result).</summary>
    public TimeSpan? FinishTime { get; set; }

    /// <summary>Finishing position of the participant (may be null).</summary>
    public int? Position { get; set; }

    /// <summary>Status of the result (for example <c>Finished</c> or <c>DNF</c>).</summary>
    public string Status { get; set; } = "Finished";
}
