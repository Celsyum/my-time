using System.Windows;
using MyTime.Services;

namespace MyTime;

public partial class App : System.Windows.Application
{
	private SettingsService? _settingsService;
	private ActivityStorageService? _activityStorageService;
	private HistoryGroupingService? _historyGroupingService;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		_settingsService = new SettingsService();
		_activityStorageService = new ActivityStorageService();
		_historyGroupingService = new HistoryGroupingService();

		var settings = _settingsService.Load();
		var cleanupService = new RetentionCleanupService();
		cleanupService.Cleanup(settings.RetentionMonths);

		var window = new MainWindow(_settingsService, _activityStorageService, _historyGroupingService, settings);
		MainWindow = window;
		window.Show();
	}
}

