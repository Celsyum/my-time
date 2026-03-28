using System.IO;
using MyTime.Models;

namespace MyTime.Services;

public sealed class RetentionCleanupService
{
    public void Cleanup(int retentionMonths)
    {
        if (retentionMonths <= 0)
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.HistoryDirectory);
        var thresholdUtc = DateTime.UtcNow.AddMonths(-retentionMonths);

        foreach (var file in Directory.EnumerateFiles(AppPaths.HistoryDirectory, "*.json"))
        {
            try
            {
                var lastWriteUtc = File.GetLastWriteTimeUtc(file);
                if (lastWriteUtc < thresholdUtc)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Ignore cleanup failures for individual files.
            }
        }
    }
}
