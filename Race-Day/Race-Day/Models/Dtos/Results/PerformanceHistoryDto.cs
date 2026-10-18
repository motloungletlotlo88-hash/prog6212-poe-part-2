namespace Race_Day.Models.Dtos.Results;

/// <summary>A single entry in a participant's personal performance history.</summary>
public class PerformanceHistoryDto
{
    /// <summary>Name of the event.</summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>Name of the category entered.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Finish time, if recorded.</summary>
    public TimeSpan? FinishTime { get; set; }

    /// <summary>Finishing position, if recorded.</summary>
    public int? Position { get; set; }
}
