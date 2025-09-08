using System.Runtime.InteropServices;

namespace SystemSoundsVolumeTray
{
    public static class CoreAudioController
    {
        private static ISimpleAudioVolume? _systemSoundsVolume;
        private static readonly object _lock = new object();

        public static void SetSystemSoundsVolume(int volume)
        {
            lock (_lock)
            {
                if (!EnsureSystemSoundsSession()) return;

                try
                {
                    float volumeFloat = volume / 100.0f;
                    Guid emptyGuid = Guid.Empty;
                    _systemSoundsVolume?.SetMasterVolume(volumeFloat, ref emptyGuid);
                }
                catch (COMException)
                {
                    ReleaseVolumeControl();
                }
            }
        }

        private static bool EnsureSystemSoundsSession()
        {
            if (_systemSoundsVolume != null)
            {
                try
                {
                    _systemSoundsVolume.GetMasterVolume(out _);
                    return true;
                }
                catch (COMException)
                {
                    ReleaseVolumeControl();
                }
            }
            
            FindAndCacheSystemSoundsSession();
            return _systemSoundsVolume != null;
        }

        private static void FindAndCacheSystemSoundsSession()
        {
            ReleaseVolumeControl(); // Освобождаем старую ссылку перед поиском новой
            IMMDeviceEnumerator? deviceEnumerator = null;
            IMMDeviceCollection? deviceCollection = null;

            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                deviceEnumerator.EnumAudioEndpoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE, out deviceCollection);
                deviceCollection.GetCount(out uint deviceCount);

                for (uint i = 0; i < deviceCount; i++)
                {
                    IMMDevice? device = null;
                    IAudioSessionManager2? sessionManager = null;
                    IAudioSessionEnumerator? sessionEnumerator = null;
                    try
                    {
                        deviceCollection.Item(i, out device);
                        Guid audioSessionManager2Guid = typeof(IAudioSessionManager2).GUID;
                        device.Activate(ref audioSessionManager2Guid, 0, IntPtr.Zero, out object sessionManagerObj);
                        sessionManager = (IAudioSessionManager2)sessionManagerObj;

                        sessionManager.GetSessionEnumerator(out sessionEnumerator);
                        sessionEnumerator.GetCount(out int sessionCount);

                        for (int j = 0; j < sessionCount; j++)
                        {
                            IAudioSessionControl? sessionControl = null;
                            try
                            {
                                sessionEnumerator.GetSession(j, out sessionControl);
                                if (sessionControl == null) continue;

                                IAudioSessionControl2? sessionControl2 = sessionControl as IAudioSessionControl2;

                                if (sessionControl2 != null && sessionControl2.IsSystemSoundsSession() == 0) // S_OK
                                {
                                    IntPtr volumePtr = IntPtr.Zero;
                                    Guid simpleAudioVolumeGuid = typeof(ISimpleAudioVolume).GUID;
                                    Marshal.QueryInterface(Marshal.GetIUnknownForObject(sessionControl), ref simpleAudioVolumeGuid, out volumePtr);
                                    
                                    if (volumePtr != IntPtr.Zero)
                                    {
                                        _systemSoundsVolume = (ISimpleAudioVolume)Marshal.GetObjectForIUnknown(volumePtr);
                                        Marshal.Release(volumePtr);
                                        // Важно: выходим из всех циклов, так как сессия найдена
                                        return;
                                    }
                                }
                            }
                            finally
                            {
                                // ПРАВИЛЬНО: Освобождаем только оригинальный объект.
                                // sessionControl2 - это просто другая "обертка" для того же объекта.
                                if (sessionControl != null) Marshal.ReleaseComObject(sessionControl);
                            }
                        }
                    }
                    finally
                    {
                        if (sessionEnumerator != null) Marshal.ReleaseComObject(sessionEnumerator);
                        if (sessionManager != null) Marshal.ReleaseComObject(sessionManager);
                        if (device != null) Marshal.ReleaseComObject(device);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки, просто не нашли сессию
            }
            finally
            {
                if (deviceCollection != null) Marshal.ReleaseComObject(deviceCollection);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }
        
        public static void ReleaseVolumeControl()
        {
            lock (_lock)
            {
                if (_systemSoundsVolume != null)
                {
                    Marshal.ReleaseComObject(_systemSoundsVolume);
                    _systemSoundsVolume = null;
                }
            }
        }
    }

    #region COM Interfaces

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(EDataFlow dataFlow, EDeviceState dwStateMask, out IMMDeviceCollection ppDevices);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator { }

    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out uint pcDevices);
        [PreserveSig] int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [ComImport, Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEvents { }

    [ComImport, Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl
    {
        [PreserveSig] int GetState(out AudioSessionState pRetVal);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid pRetVal);
        [PreserveSig] int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        [PreserveSig] int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
    }

    [ComImport, Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2 : IAudioSessionControl
    {
        [PreserveSig] new int GetState(out AudioSessionState pRetVal);
        [PreserveSig] new int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] new int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] new int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] new int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] new int GetGroupingParam(out Guid pRetVal);
        [PreserveSig] new int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig] new int RegisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        [PreserveSig] new int UnregisterAudioSessionNotification(IAudioSessionEvents NewNotifications);
        [PreserveSig] int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int GetProcessId(out uint pRetVal);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference(bool optOut);
    }

    [ComImport, Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        [PreserveSig] int GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags, out IAudioSessionControl SessionControl);
        [PreserveSig] int GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags, out ISimpleAudioVolume AudioVolume);
        [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    }

    [ComImport, Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int SessionCount);
        [PreserveSig] int GetSession(int SessionCount, out IAudioSessionControl Session);
    }

    [ComImport, Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig] int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig] int GetMasterVolume(out float pfLevel);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid EventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
    }

    internal enum EDataFlow { eRender, eCapture, eAll }
    internal enum EDeviceState { DEVICE_STATE_ACTIVE = 0x1 }
    internal enum AudioSessionState { AudioSessionStateInactive = 0, AudioSessionStateActive = 1, AudioSessionStateExpired = 2 }

    #endregion
}
