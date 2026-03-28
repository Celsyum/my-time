using System.Globalization;
using MyTime.Models;

namespace MyTime.Services;

public sealed class HistoryGroupingService
{
    public IReadOnlyList<HistoryDisplayItem> BuildDisplayItems(IEnumerable<ActivityRecord> activities, GroupByPeriod groupBy)
    {
        var records = activities
            .OrderByDescending(x => x.StartUtc)
            .ToList();

        var grouped = records
            .GroupBy(x => GetGroupKey(x.StartUtc, groupBy))
            .OrderByDescending(x => x.Key.SortUtc);

        var items = new List<HistoryDisplayItem>();

        foreach (var group in grouped)
        {
            var totalSeconds = group.Sum(x => x.DurationSeconds);
            items.Add(new HistoryDisplayItem
            {
                IsHeader = true,
                Text = $"{group.Key.Label} | Total: {FormatDuration(totalSeconds)}"
            });

            foreach (var record in group.OrderByDescending(x => x.StartUtc))
            {
                items.Add(new HistoryDisplayItem
                {
                    IsHeader = false,
                    Text = $"{record.StartUtc.ToLocalTime():yyyy-MM-dd HH:mm} - {FormatDuration(record.DurationSeconds)} - {record.Description}"
                });
            }
        }

        return items;
    }

    public static string FormatDuration(long durationSeconds)
    {
        var ts = TimeSpan.FromSeconds(durationSeconds);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    private static (DateTime SortUtc, string Label) GetGroupKey(DateTime startUtc, GroupByPeriod groupBy)
    {
        var local = startUtc.ToLocalTime();

        return groupBy switch
        {
            GroupByPeriod.Day => (
                new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime(),
                local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            GroupByPeriod.Week => GetWeekKey(local),
            GroupByPeriod.Month => (
                new DateTime(local.Year, local.Month, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime(),
                local.ToString("yyyy-MM", CultureInfo.InvariantCulture)),
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
        };
    }

    private static (DateTime SortUtc, string Label) GetWeekKey(DateTime localDateTime)
    {
        var date = localDateTime.Date;
        var dayOfWeek = (int)date.DayOfWeek;
        var distanceToMonday = (dayOfWeek + 6) % 7;
        var monday = date.AddDays(-distanceToMonday);

        var weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            monday,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        return (
            monday.ToUniversalTime(),
            $"{monday.Year}-W{weekNumber:00}");
    }
}
