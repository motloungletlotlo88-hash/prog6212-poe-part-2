namespace Race_Day.Models.Dtos.Results;

/// <summary>Representation of a captured result.</summary>
public class ResultDto
{
    /// <summary>Result identifier.</summary>
    public int ResultId { get; set; }

    /// <summary>Identifier of the enrolment the result belongs to.</summary>
    public int EnrolmentId { get; set; }

    /// <summary>Finish time, if recorded.</summary>
    public TimeSpan? FinishTime { get; set; }

    /// <summary>Finishing position, if recorded.</summary>
    public int? Position { get; set; }

    /// <summary>Status of the result.</summary>
    public string Status { get; set; } = string.Empty;
}
