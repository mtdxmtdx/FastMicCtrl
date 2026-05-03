namespace FastMicCtrl;

public sealed class HotkeyGesture
{
    public static HotkeyGesture Default => new()
    {
        Control = true,
        Alt = true,
        Shift = false,
        Key = Keys.M
    };

    public bool Control { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public Keys Key { get; set; }

    public bool IsValid =>
        Key != Keys.None &&
        Key != Keys.ControlKey &&
        Key != Keys.ShiftKey &&
        Key != Keys.Menu &&
        (Control || Alt || Shift);

    public int ToModifierFlags()
    {
        var flags = 0;

        if (Alt)
        {
            flags |= 0x0001;
        }

        if (Control)
        {
            flags |= 0x0002;
        }

        if (Shift)
        {
            flags |= 0x0004;
        }

        return flags;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (Control)
        {
            parts.Add("Ctrl");
        }

        if (Alt)
        {
            parts.Add("Alt");
        }

        if (Shift)
        {
            parts.Add("Shift");
        }

        if (Key != Keys.None)
        {
            parts.Add(Key.ToString());
        }

        return string.Join(" + ", parts);
    }

    public static HotkeyGesture? FromKeyEvent(KeyEventArgs e)
    {
        var key = e.KeyCode;

        if (key is Keys.ControlKey or Keys.ShiftKey or Keys.Menu)
        {
            return null;
        }

        var gesture = new HotkeyGesture
        {
            Control = e.Control,
            Alt = e.Alt,
            Shift = e.Shift,
            Key = key
        };

        return gesture.IsValid ? gesture : null;
    }
}
