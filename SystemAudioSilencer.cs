using System.Runtime.InteropServices;

namespace ScreenshotCapture;

internal sealed class SystemAudioSilencer : IDisposable
{
    private readonly IAudioEndpointVolume _endpoint;
    private readonly float _previousVolume;
    private readonly bool _previousMute;
    private bool _disposed;

    private SystemAudioSilencer(IAudioEndpointVolume endpoint, float previousVolume, bool previousMute)
    {
        _endpoint = endpoint;
        _previousVolume = previousVolume;
        _previousMute = previousMute;
    }

    public static SystemAudioSilencer MuteMasterEndpoint()
    {
        var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
        try
        {
            Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device));
            try
            {
                var endpointId = typeof(IAudioEndpointVolume).GUID;
                Marshal.ThrowExceptionForHR(device.Activate(ref endpointId, 23, IntPtr.Zero, out var activated));
                var endpoint = (IAudioEndpointVolume)activated;
                Marshal.ThrowExceptionForHR(endpoint.GetMasterVolumeLevelScalar(out var volume));
                Marshal.ThrowExceptionForHR(endpoint.GetMute(out var muted));
                Marshal.ThrowExceptionForHR(endpoint.SetMasterVolumeLevelScalar(0, Guid.Empty));
                return new SystemAudioSilencer(endpoint, volume, muted);
            }
            finally { Marshal.ReleaseComObject(device); }
        }
        finally { Marshal.ReleaseComObject(enumerator); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            _ = _endpoint.SetMasterVolumeLevelScalar(_previousVolume, Guid.Empty);
            _ = _endpoint.SetMute(_previousMute, Guid.Empty);
        }
        finally
        {
            Marshal.ReleaseComObject(_endpoint);
            _disposed = true;
        }
    }

    private enum EDataFlow { eRender, eCapture, eAll }
    private enum ERole { eConsole, eMultimedia, eCommunications }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(EDataFlow dataFlow, int stateMask, out object devices);
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
        int GetDevice(string id, out IMMDevice device);
        int RegisterEndpointNotificationCallback(nint client);
        int UnregisterEndpointNotificationCallback(nint client);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        int Activate(ref Guid iid, int clsContext, nint activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
        int OpenPropertyStore(int access, out nint properties);
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        int GetState(out int state);
    }

    [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(nint notify);
        int UnregisterControlChangeNotify(nint notify);
        int GetChannelCount(out uint channelCount);
        int SetMasterVolumeLevel(float levelDb, Guid eventContext);
        int SetMasterVolumeLevelScalar(float level, Guid eventContext);
        int GetMasterVolumeLevel(out float levelDb);
        int GetMasterVolumeLevelScalar(out float level);
        int SetChannelVolumeLevel(uint channel, float levelDb, Guid eventContext);
        int SetChannelVolumeLevelScalar(uint channel, float level, Guid eventContext);
        int GetChannelVolumeLevel(uint channel, out float levelDb);
        int GetChannelVolumeLevelScalar(uint channel, out float level);
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, Guid eventContext);
        int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
        int GetVolumeStepInfo(out uint step, out uint stepCount);
        int VolumeStepUp(Guid eventContext);
        int VolumeStepDown(Guid eventContext);
        int QueryHardwareSupport(out uint hardwareSupportMask);
        int GetVolumeRange(out float minDb, out float maxDb, out float incrementDb);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumeratorComObject { }
}
