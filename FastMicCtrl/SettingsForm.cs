namespace FastMicCtrl;

public sealed class SettingsForm : Form
{
    private readonly Label _hotkeyDescriptionLabel;
    private readonly TextBox _hotkeyTextBox;
    private readonly CheckBox _startupCheckBox;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;

    private HotkeyGesture _selectedHotkey = HotkeyGesture.Default;
    private bool _allowClose;

    public event EventHandler<AppSettings>? SettingsSaved;

    public SettingsForm()
    {
        Text = "FastMicCtrl 设置";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        ClientSize = new Size(360, 190);

        var titleLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 20),
            Text = "切换全部麦克风状态的全局快捷键"
        };

        _hotkeyTextBox = new TextBox
        {
            Location = new Point(20, 52),
            Width = 320,
            ReadOnly = true,
            TabStop = true
        };
        _hotkeyTextBox.KeyDown += HotkeyTextBox_KeyDown;

        _hotkeyDescriptionLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 84),
            Text = "点击输入框后按下新的组合键，例如 Ctrl + Alt + M"
        };

        _startupCheckBox = new CheckBox
        {
            AutoSize = true,
            Location = new Point(20, 115),
            Text = "开机自动启动"
        };

        _saveButton = new Button
        {
            Text = "保存",
            Location = new Point(184, 145),
            Width = 75
        };
        _saveButton.Click += SaveButton_Click;

        _cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(265, 145),
            Width = 75
        };
        _cancelButton.Click += (_, _) => Hide();

        Controls.Add(titleLabel);
        Controls.Add(_hotkeyTextBox);
        Controls.Add(_hotkeyDescriptionLabel);
        Controls.Add(_startupCheckBox);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
    }

    public void LoadSettings(AppSettings settings)
    {
        _selectedHotkey = settings.Hotkey;
        _hotkeyTextBox.Text = settings.Hotkey.ToString();
        _startupCheckBox.Checked = settings.StartWithWindows;
    }

    public void PrepareForExit()
    {
        _allowClose = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnFormClosing(e);
    }

    private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;
        e.Handled = true;

        var gesture = HotkeyGesture.FromKeyEvent(e);

        if (gesture is null)
        {
            _hotkeyDescriptionLabel.Text = "请使用 Ctrl / Alt / Shift 与其他按键组合";
            return;
        }

        _selectedHotkey = gesture;
        _hotkeyTextBox.Text = gesture.ToString();
        _hotkeyDescriptionLabel.Text = "快捷键已更新，点击保存后生效";
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!_selectedHotkey.IsValid)
        {
            MessageBox.Show(
                this,
                "请选择一个包含 Ctrl、Alt、Shift 中至少一个修饰键的组合键。",
                "快捷键无效",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        SettingsSaved?.Invoke(this, new AppSettings
        {
            StartWithWindows = _startupCheckBox.Checked,
            Hotkey = _selectedHotkey
        });
    }
}
