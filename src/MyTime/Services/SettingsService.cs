using System.IO;
using System.Text.Json;
using MyTime.Models;

namespace MyTime.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings Load()
    {
        EnsureDirectories();

        if (!File.Exists(AppPaths.SettingsFile))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(AppPaths.SettingsFile);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings is null || settings.RetentionMonths <= 0)
            {
                return new AppSettings();
            }

            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        EnsureDirectories();

        if (settings.RetentionMonths <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings.RetentionMonths), "Retention must be greater than zero.");
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(AppPaths.SettingsFile, json);
    }

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppPaths.BaseDirectory);
        Directory.CreateDirectory(AppPaths.HistoryDirectory);
    }
}
