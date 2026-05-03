using Microsoft.Win32;

namespace FastMicCtrl;

public sealed class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "FastMicCtrl";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(AppName) as string;

        return string.Equals(value, BuildRunValue(), StringComparison.OrdinalIgnoreCase);
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (enabled)
        {
            key.SetValue(AppName, BuildRunValue());
            return;
        }

        key.DeleteValue(AppName, throwOnMissingValue: false);
    }

    private static string BuildRunValue()
    {
        return $"\"{Application.ExecutablePath}\"";
    }
}
