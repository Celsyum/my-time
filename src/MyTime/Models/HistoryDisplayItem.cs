namespace MyTime.Models;

public sealed class HistoryDisplayItem
{
    public bool IsHeader { get; init; }

    public string Text { get; init; } = string.Empty;
}
