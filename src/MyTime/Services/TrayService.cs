using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MyTime.Services;

public sealed class TrayService : IDisposable
{
    private const string CustomIconRelativePath = "Assets\\Icons\\mytime.ico";
    private readonly NotifyIcon _notifyIcon;

    public TrayService(Action onShowHide, Action onExit)
    {
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show / Hide", null, (_, _) => onShowHide());
        contextMenu.Items.Add("Exit", null, (_, _) => onExit());

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Visible = true,
            Text = "MyTime",
            ContextMenuStrip = contextMenu
        };

        _notifyIcon.DoubleClick += (_, _) => onShowHide();
    }

    public void SetTooltip(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _notifyIcon.Text = "MyTime";
            return;
        }

        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    public void ShowInfoNotification(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            _notifyIcon.BalloonTipTitle = "MyTime";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(3000);
        }
        catch
        {
            // Ignore notification failures so timer behavior stays unaffected.
        }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private static Icon LoadTrayIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, CustomIconRelativePath);
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch
        {
            // Fall through to default icon.
        }

        return SystemIcons.Application;
    }
}
