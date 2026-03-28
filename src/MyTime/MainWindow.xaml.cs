using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MyTime.Models;
using MyTime.Services;

namespace MyTime;

public partial class MainWindow : Window
{
    private static readonly TimeSpan RunningReminderInterval = TimeSpan.FromMinutes(15);

    private readonly SettingsService _settingsService;
    private readonly ActivityStorageService _activityStorageService;
    private readonly HistoryGroupingService _historyGroupingService;
    private readonly DispatcherTimer _uiTimer;
    private readonly ObservableCollection<HistoryDisplayItem> _historyItems = new();

    private readonly AppSettings _settings;
    private TrayService? _trayService;
    private DateTime? _runningStartUtc;
    private DateTime? _nextRunningReminderUtc;
    private long _lastElapsedSeconds;
    private bool _awaitingDescription;
    private bool _isExiting;

    public MainWindow(
        SettingsService settingsService,
        ActivityStorageService activityStorageService,
        HistoryGroupingService historyGroupingService,
        AppSettings settings)
    {
        _settingsService = settingsService;
        _activityStorageService = activityStorageService;
        _historyGroupingService = historyGroupingService;
        _settings = settings;

        InitializeComponent();

        HistoryListBox.ItemsSource = _historyItems;
        RetentionMonthsTextBox.Text = _settings.RetentionMonths.ToString();

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uiTimer.Tick += (_, _) => UpdateElapsedDisplay();

        _trayService = new TrayService(ToggleWindowVisibility, ExitApplication);
        RefreshHistory();
        UpdateUiState();
    }

    private bool IsRunning => _runningStartUtc.HasValue && !_awaitingDescription;

    private void StartStop_Click(object sender, RoutedEventArgs e)
    {
        ValidationText.Text = string.Empty;

        if (!IsRunning)
        {
            StartTimer();
            return;
        }

        StopTimerAndAskDescription();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!_awaitingDescription)
        {
            return;
        }

        var description = DescriptionTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            ValidationText.Text = "Description cannot be empty.";
            return;
        }

        SaveCompletedActivity(description, DateTime.UtcNow, _lastElapsedSeconds);
        ResetAfterSave();
    }

    private void GroupByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshHistory();
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsValidationText.Text = string.Empty;
        RetentionMonthsTextBox.Text = _settings.RetentionMonths.ToString();
        SettingsOverlay.Visibility = Visibility.Visible;
    }

    private void QuitApp_Click(object sender, RoutedEventArgs e)
    {
        if (IsRunning)
        {
            return;
        }

        ExitApplication();
    }

    private void CloseSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsValidationText.Text = string.Empty;

        if (!int.TryParse(RetentionMonthsTextBox.Text.Trim(), out var retentionMonths) || retentionMonths <= 0)
        {
            SettingsValidationText.Text = "Retention months must be a positive whole number.";
            return;
        }

        _settings.RetentionMonths = retentionMonths;
        _settingsService.Save(_settings);
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
            WindowState = WindowState.Minimized;
            return;
        }

        if (IsRunning && _runningStartUtc.HasValue)
        {
            var exitUtc = DateTime.UtcNow;
            var seconds = Math.Max(1, (long)(exitUtc - _runningStartUtc.Value).TotalSeconds);
            var exitLocal = exitUtc.ToLocalTime();
            var description = $"Auto-stopped on full exit at {exitLocal:yyyy-MM-dd HH:mm:ss}";
            SaveCompletedActivity(description, exitUtc, seconds);
        }

        _trayService?.Dispose();
        _trayService = null;

        base.OnClosing(e);
    }

    private void StartTimer()
    {
        _runningStartUtc = DateTime.UtcNow;
        _nextRunningReminderUtc = DateTime.UtcNow.Add(RunningReminderInterval);
        _lastElapsedSeconds = 0;
        _awaitingDescription = false;
        DescriptionTextBox.Text = string.Empty;
        _uiTimer.Start();
        UpdateUiState();
    }

    private void StopTimerAndAskDescription()
    {
        if (!_runningStartUtc.HasValue)
        {
            return;
        }

        _uiTimer.Stop();
        _lastElapsedSeconds = Math.Max(1, (long)(DateTime.UtcNow - _runningStartUtc.Value).TotalSeconds);
        _awaitingDescription = true;
        UpdateElapsedDisplay();
        UpdateUiState();
    }

    private void ResetAfterSave()
    {
        _runningStartUtc = null;
        _nextRunningReminderUtc = null;
        _lastElapsedSeconds = 0;
        _awaitingDescription = false;
        DescriptionTextBox.Text = string.Empty;
        ValidationText.Text = string.Empty;
        UpdateElapsedDisplay();
        UpdateUiState();
        RefreshHistory();
    }

    private void SaveCompletedActivity(string description, DateTime endUtc, long durationSeconds)
    {
        if (!_runningStartUtc.HasValue)
        {
            return;
        }

        var record = new ActivityRecord
        {
            StartUtc = _runningStartUtc.Value,
            EndUtc = endUtc,
            DurationSeconds = durationSeconds,
            Description = description
        };

        _activityStorageService.Save(record);
    }

    private void RefreshHistory()
    {
        var records = _activityStorageService.GetAll();
        var groupBy = GetSelectedGroupBy();
        var items = _historyGroupingService.BuildDisplayItems(records, groupBy);

        _historyItems.Clear();
        foreach (var item in items)
        {
            _historyItems.Add(item);
        }
    }

    private GroupByPeriod GetSelectedGroupBy()
    {
        var selected = (GroupByComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        return selected switch
        {
            "Week" => GroupByPeriod.Week,
            "Month" => GroupByPeriod.Month,
            _ => GroupByPeriod.Day
        };
    }

    private void UpdateElapsedDisplay()
    {
        if (IsRunning && _runningStartUtc.HasValue)
        {
            var nowUtc = DateTime.UtcNow;
            var elapsedSeconds = Math.Max(0, (long)(nowUtc - _runningStartUtc.Value).TotalSeconds);
            _lastElapsedSeconds = elapsedSeconds;
            ElapsedText.Text = HistoryGroupingService.FormatDuration(elapsedSeconds);
            _trayService?.SetTooltip($"MyTime running {ElapsedText.Text}");

            if (_nextRunningReminderUtc.HasValue && nowUtc >= _nextRunningReminderUtc.Value)
            {
                _trayService?.ShowInfoNotification("Work Activity in Progress...");

                while (_nextRunningReminderUtc.HasValue && nowUtc >= _nextRunningReminderUtc.Value)
                {
                    _nextRunningReminderUtc = _nextRunningReminderUtc.Value.Add(RunningReminderInterval);
                }
            }

            return;
        }

        ElapsedText.Text = HistoryGroupingService.FormatDuration(_lastElapsedSeconds);
        _trayService?.SetTooltip("MyTime");
    }

    private void UpdateUiState()
    {
        QuitAppButton.IsEnabled = !IsRunning;

        if (IsRunning)
        {
            StartStopButton.Content = "Stop";
            DescriptionPanel.Visibility = Visibility.Collapsed;
            ValidationText.Text = string.Empty;
            return;
        }

        if (_awaitingDescription)
        {
            StartStopButton.Content = "Start";
            DescriptionPanel.Visibility = Visibility.Visible;
            return;
        }

        StartStopButton.Content = "Start";
        DescriptionPanel.Visibility = Visibility.Collapsed;
    }

    private void ToggleWindowVisibility()
    {
        if (IsVisible)
        {
            Hide();
            ShowInTaskbar = false;
            return;
        }

        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        Close();
    }
}