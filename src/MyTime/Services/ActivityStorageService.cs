using System.IO;
using System.Text.Json;
using MyTime.Models;

namespace MyTime.Services;

public sealed class ActivityStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public ActivityStorageService()
    {
        Directory.CreateDirectory(AppPaths.HistoryDirectory);
    }

    public void Save(ActivityRecord record)
    {
        var filePath = Path.Combine(AppPaths.HistoryDirectory, $"{record.Id}.json");
        var json = JsonSerializer.Serialize(record, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public IReadOnlyList<ActivityRecord> GetAll()
    {
        Directory.CreateDirectory(AppPaths.HistoryDirectory);

        var records = new List<ActivityRecord>();
        foreach (var file in Directory.EnumerateFiles(AppPaths.HistoryDirectory, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var record = JsonSerializer.Deserialize<ActivityRecord>(json);
                if (record is null)
                {
                    continue;
                }

                records.Add(record);
            }
            catch
            {
                // Ignore malformed files so one bad file does not break history rendering.
            }
        }

        return records
            .OrderByDescending(x => x.StartUtc)
            .ToList();
    }
}
