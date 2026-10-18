namespace Race_Day.Models.Dtos.Enrolments;

/// <summary>An enrolment as viewed by the organiser that owns the event.</summary>
public class CategoryEnrolmentDto
{
    /// <summary>Enrolment identifier.</summary>
    public int EnrolmentId { get; set; }

    /// <summary>Full name of the enrolled participant.</summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>Status of the enrolment.</summary>
    public string Status { get; set; } = string.Empty;
}
