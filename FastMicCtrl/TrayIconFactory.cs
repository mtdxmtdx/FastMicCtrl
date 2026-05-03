using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace FastMicCtrl;

public static class TrayIconFactory
{
    public static Icon Create(MicrophoneMuteState state)
    {
        var color = state switch
        {
            MicrophoneMuteState.Unmuted => Color.FromArgb(34, 197, 94),
            MicrophoneMuteState.Muted => Color.FromArgb(239, 68, 68),
            MicrophoneMuteState.Mixed => Color.FromArgb(245, 158, 11),
            _ => Color.FromArgb(107, 114, 128)
        };

        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var brush = new SolidBrush(color);
        using var pen = new Pen(Color.White, 2.6f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        graphics.FillEllipse(brush, 2, 2, 28, 28);

        graphics.DrawLine(pen, 16, 8, 16, 20);
        graphics.DrawArc(pen, 10, 6, 12, 12, 180, 180);
        graphics.DrawLine(pen, 11, 22, 21, 22);

        if (state == MicrophoneMuteState.Muted)
        {
            graphics.DrawLine(pen, 8, 24, 24, 8);
        }

        var handle = bitmap.GetHicon();

        try
        {
            using var icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
