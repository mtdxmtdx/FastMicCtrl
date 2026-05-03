using System.Runtime.InteropServices;

namespace FastMicCtrl;

public sealed class HotkeyManager : IDisposable
{
    private const int HotkeyId = 1;
    private const int ModNoRepeat = 0x4000;

    private readonly HotkeyWindow _window = new();
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public HotkeyManager()
    {
        _window.HotkeyPressed += (_, _) => HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public bool Register(HotkeyGesture gesture)
    {
        Unregister();

        if (!gesture.IsValid)
        {
            return false;
        }

        _registered = RegisterHotKey(
            _window.Handle,
            HotkeyId,
            gesture.ToModifierFlags() | ModNoRepeat,
            (uint)gesture.Key);

        return _registered;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_window.Handle, HotkeyId);
        _registered = false;
    }

    public void Dispose()
    {
        Unregister();
        _window.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WmHotkey = 0x0312;

        public event EventHandler? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
