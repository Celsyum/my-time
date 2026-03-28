using System.IO;

namespace MyTime.Services;

public static class AppPaths
{
    public static string BaseDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyTime");

    public static string HistoryDirectory => Path.Combine(BaseDirectory, "history");

    public static string SettingsFile => Path.Combine(BaseDirectory, "settings.json");
}
