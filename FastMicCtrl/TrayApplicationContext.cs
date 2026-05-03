using System.Media;

namespace FastMicCtrl;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly AppSettingsStore _settingsStore = new();
    private readonly StartupManager _startupManager = new();
    private readonly HotkeyManager _hotkeyManager = new();
    private readonly MicrophoneController _microphoneController = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _exitItem;
    private readonly SettingsForm _settingsForm;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    private AppSettings _settings;
    private Icon? _currentIcon;

    public TrayApplicationContext()
    {
        _settings = _settingsStore.Load();
        _settings.StartWithWindows = _startupManager.IsEnabled();

        _statusItem = new ToolStripMenuItem("正在检测麦克风状态") { Enabled = false };
        _toggleItem = new ToolStripMenuItem("切换全部麦克风");
        _startupItem = new ToolStripMenuItem("开机自启动");
        _settingsItem = new ToolStripMenuItem("设置...");
        _exitItem = new ToolStripMenuItem("退出");

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.AddRange(
            [
                _statusItem,
                new ToolStripSeparator(),
                _toggleItem,
                _startupItem,
                _settingsItem,
                new ToolStripSeparator(),
                _exitItem
            ]);
        _contextMenu.Opening += (_, _) => RefreshUi();

        _notifyIcon = new NotifyIcon
        {
            Visible = true,
            ContextMenuStrip = _contextMenu
        };
        _notifyIcon.DoubleClick += (_, _) => ShowSettings();

        _toggleItem.Click += (_, _) => ToggleMicrophones();
        _startupItem.Click += (_, _) => ToggleStartup();
        _settingsItem.Click += (_, _) => ShowSettings();
        _exitItem.Click += (_, _) => ExitApplication();

        _settingsForm = new SettingsForm();
        _settingsForm.LoadSettings(_settings);
        _settingsForm.SettingsSaved += SettingsForm_SettingsSaved;

        _hotkeyManager.HotkeyPressed += (_, _) => ToggleMicrophones();

        if (!TryRegisterHotkey(_settings.Hotkey))
        {
            MessageBox.Show(
                "默认快捷键注册失败，请打开设置改成其他组合键。",
                "FastMicCtrl",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        _refreshTimer = new System.Windows.Forms.Timer
        {
            Interval = 2000,
            Enabled = true
        };
        _refreshTimer.Tick += (_, _) => RefreshUi();

        RefreshUi();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer.Dispose();
            _hotkeyManager.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
            _currentIcon?.Dispose();

            if (!_settingsForm.IsDisposed)
            {
                _settingsForm.PrepareForExit();
                _settingsForm.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private void SettingsForm_SettingsSaved(object? sender, AppSettings newSettings)
    {
        var previousSettings = _settings;

        if (!TryRegisterHotkey(newSettings.Hotkey))
        {
            TryRegisterHotkey(previousSettings.Hotkey);
            MessageBox.Show(
                _settingsForm,
                "新快捷键注册失败，可能已经被其他程序占用。",
                "保存失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _startupManager.SetEnabled(newSettings.StartWithWindows);

        _settings = newSettings;
        _settingsStore.Save(_settings);
        _settingsForm.LoadSettings(_settings);
        _settingsForm.Hide();
        RefreshUi();
    }

    private bool TryRegisterHotkey(HotkeyGesture gesture)
    {
        return _hotkeyManager.Register(gesture);
    }

    private void ToggleMicrophones()
    {
        var state = _microphoneController.ToggleAll();

        switch (state)
        {
            case MicrophoneMuteState.Muted:
                SystemSounds.Exclamation.Play();
                break;
            case MicrophoneMuteState.Unmuted:
                SystemSounds.Asterisk.Play();
                break;
            case MicrophoneMuteState.NoDevice:
                SystemSounds.Hand.Play();
                break;
        }

        RefreshUi();
    }

    private void ToggleStartup()
    {
        _settings.StartWithWindows = !_settings.StartWithWindows;
        _startupManager.SetEnabled(_settings.StartWithWindows);
        _settingsStore.Save(_settings);
        _settingsForm.LoadSettings(_settings);
        RefreshUi();
    }

    private void ShowSettings()
    {
        _settingsForm.LoadSettings(_settings);
        _settingsForm.Show();
        _settingsForm.BringToFront();
        _settingsForm.Activate();
    }

    private void ExitApplication()
    {
        ExitThread();
    }

    private void RefreshUi()
    {
        var state = _microphoneController.GetState();

        _statusItem.Text = state switch
        {
            MicrophoneMuteState.Muted => "状态: 全部麦克风已关闭",
            MicrophoneMuteState.Unmuted => "状态: 全部麦克风已开启",
            MicrophoneMuteState.Mixed => "状态: 麦克风状态不一致",
            _ => "状态: 未检测到可用麦克风"
        };

        _toggleItem.Text = state == MicrophoneMuteState.Muted ? "开启全部麦克风" : "关闭全部麦克风";
        _toggleItem.Enabled = state != MicrophoneMuteState.NoDevice;
        _startupItem.Checked = _settings.StartWithWindows;

        SetTrayIcon(state);
    }

    private void SetTrayIcon(MicrophoneMuteState state)
    {
        var toolTip = state switch
        {
            MicrophoneMuteState.Muted => "FastMicCtrl - 麦克风已关闭",
            MicrophoneMuteState.Unmuted => "FastMicCtrl - 麦克风已开启",
            MicrophoneMuteState.Mixed => "FastMicCtrl - 麦克风状态不一致",
            _ => "FastMicCtrl - 未检测到麦克风"
        };

        _notifyIcon.Text = toolTip;

        _currentIcon?.Dispose();
        _currentIcon = TrayIconFactory.Create(state);
        _notifyIcon.Icon = _currentIcon;
    }
}
