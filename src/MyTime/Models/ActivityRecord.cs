namespace MyTime.Models;

public sealed class ActivityRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public string Description { get; set; } = string.Empty;

    public long DurationSeconds { get; set; }
}
