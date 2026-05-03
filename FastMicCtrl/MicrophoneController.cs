using NAudio.CoreAudioApi;

namespace FastMicCtrl;

public sealed class MicrophoneController
{
    public MicrophoneMuteState GetState()
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = GetActiveCaptureDevices(enumerator).ToList();

        if (devices.Count == 0)
        {
            return MicrophoneMuteState.NoDevice;
        }

        var mutedCount = 0;

        foreach (var device in devices)
        {
            if (device.AudioEndpointVolume.Mute)
            {
                mutedCount++;
            }
        }

        if (mutedCount == 0)
        {
            return MicrophoneMuteState.Unmuted;
        }

        if (mutedCount == devices.Count)
        {
            return MicrophoneMuteState.Muted;
        }

        return MicrophoneMuteState.Mixed;
    }

    public MicrophoneMuteState ToggleAll()
    {
        var currentState = GetState();

        if (currentState == MicrophoneMuteState.NoDevice)
        {
            return currentState;
        }

        var mute = currentState != MicrophoneMuteState.Muted;
        SetAllMuted(mute);

        return GetState();
    }

    public void SetAllMuted(bool muted)
    {
        using var enumerator = new MMDeviceEnumerator();

        foreach (var device in GetActiveCaptureDevices(enumerator))
        {
            device.AudioEndpointVolume.Mute = muted;
        }
    }

    private static IEnumerable<MMDevice> GetActiveCaptureDevices(MMDeviceEnumerator enumerator)
    {
        return enumerator
            .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .Where(device => device.State == DeviceState.Active);
    }
}

public enum MicrophoneMuteState
{
    NoDevice,
    Unmuted,
    Muted,
    Mixed
}
