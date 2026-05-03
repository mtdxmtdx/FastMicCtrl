using System.Text.Json.Serialization;

namespace FastMicCtrl;

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; }

    public HotkeyGesture Hotkey { get; set; } = HotkeyGesture.Default;

    [JsonIgnore]
    public static AppSettings Default => new()
    {
        StartWithWindows = false,
        Hotkey = HotkeyGesture.Default
    };
}
