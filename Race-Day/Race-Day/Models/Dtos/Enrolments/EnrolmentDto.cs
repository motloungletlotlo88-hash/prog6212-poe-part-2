namespace Race_Day.Models.Dtos.Enrolments;

/// <summary>Response returned after a participant enrols in a category.</summary>
public class EnrolmentDto
{
    /// <summary>Enrolment identifier.</summary>
    public int EnrolmentId { get; set; }

    /// <summary>Identifier of the category entered.</summary>
    public int CategoryId { get; set; }

    /// <summary>Status of the enrolment.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the enrolment.</summary>
    public DateTime EnrolmentDate { get; set; }
}
