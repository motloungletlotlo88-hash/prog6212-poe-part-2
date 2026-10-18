namespace Race_Day.Models;

/// <summary>
/// A Participant's enrolment into a specific <see cref="Category"/>. A participant may
/// only enrol once per category, and an enrolment may have at most one <see cref="Result"/>.
/// </summary>
public class Enrolment
{
    /// <summary>Primary key.</summary>
    public int EnrolmentID { get; set; }

    /// <summary>Foreign key to the participating <see cref="User"/>.</summary>
    public int ParticipantID { get; set; }

    /// <summary>Navigation property to the participating user.</summary>
    public User? Participant { get; set; }

    /// <summary>Foreign key to the <see cref="Category"/> being entered.</summary>
    public int CategoryID { get; set; }

    /// <summary>Navigation property to the category being entered.</summary>
    public Category? Category { get; set; }

    /// <summary>UTC timestamp of when the enrolment was made.</summary>
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>Current status of the enrolment (for example <c>Confirmed</c> or <c>Cancelled</c>).</summary>
    public string Status { get; set; } = "Confirmed";

    /// <summary>The result captured for this enrolment, if any.</summary>
    public Result? Result { get; set; }
}
