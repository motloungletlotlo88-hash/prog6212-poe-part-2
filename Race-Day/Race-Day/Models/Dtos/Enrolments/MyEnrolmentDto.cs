namespace Race_Day.Models.Dtos.Enrolments;

/// <summary>A single enrolment in the logged-in participant's own enrolment list.</summary>
public class MyEnrolmentDto
{
    /// <summary>Enrolment identifier.</summary>
    public int EnrolmentId { get; set; }

    /// <summary>Name of the event the enrolment belongs to.</summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>Name of the category entered.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Status of the enrolment.</summary>
    public string Status { get; set; } = string.Empty;
}
