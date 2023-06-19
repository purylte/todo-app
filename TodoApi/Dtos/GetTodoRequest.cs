namespace TodoApi.Dtos;

public enum SortByOption
{
    TimeCreated,
    LastUpdated,
    Body,
    Done,
}

public enum SortDirection
{
    Asc,
    Desc,
}

public class DateRange
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}

public class GetTodoRequest
{
    public SortByOption SortBy { get; set; } = SortByOption.TimeCreated;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public DateRange? TimeCreatedRange { get; set; }
    public DateRange? LastUpdatedRange { get; set; }
    public bool? IsDone { get; set; }
}
