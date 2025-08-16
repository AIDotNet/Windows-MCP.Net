using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using Interface;

namespace WindowsMCP.Net.Services;

/// <summary>
/// Implementation of Windows system control services.
/// Provides methods for controlling volume, screen brightness, and display resolution.
/// </summary>
public class SystemControlService : ISystemControlService
{
    private readonly ILogger<SystemControlService> _logger;

    public SystemControlService(ILogger<SystemControlService> logger)
    {
        _logger = logger;
    }

    #region Volume Control

    /// <summary>
    /// Adjust system volume up or down.
    /// </summary>
    /// <param name="inc">true to increase volume, false to decrease</param>
    /// <returns>Result message</returns>
    public async Task<string> SetVolumeAsync(bool inc)
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            
            float currentVolume = await GetCurrentVolumeAsync();

            if (inc)
            {
                // Increase volume by 20%
                device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Min(1.0f, currentVolume + 0.2f);
                await SetMuteStateAsync(false);
            }
            else
            {
                // Decrease volume by 20%
                device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Max(0.0f, currentVolume - 0.2f);

                if (await GetCurrentVolumeAsync() == 0f)
                {
                    await SetMuteStateAsync(true);
                }
            }

            _logger.LogInformation($"Volume adjusted: {(inc ? "increased" : "decreased")}");
            return "Volume adjustment successful";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to adjust volume");
            return "Failed to adjust volume";
        }
    }

    /// <summary>
    /// Set system volume to a specific percentage.
    /// </summary>
    /// <param name="percent">Volume percentage (0-100)</param>
    /// <returns>Result message</returns>
    public async Task<string> SetVolumePercentAsync(int percent)
    {
        try
        {
            float beforeVolume = await GetCurrentVolumeAsync();

            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float volume = Math.Clamp(percent, 0, 100) / 100.0f;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;

            float afterVolume = await GetCurrentVolumeAsync();

            // Unmute when volume is increased
            if (afterVolume > beforeVolume)
            {
                await SetMuteStateAsync(false);
            }

            _logger.LogInformation($"Volume set to {percent}%");
            return "Volume set successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set volume percentage");
            return "Failed to set volume";
        }
    }

    /// <summary>
    /// Get current system volume level.
    /// </summary>
    /// <returns>Current volume as a float (0.0 to 1.0)</returns>
    public async Task<float> GetCurrentVolumeAsync()
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device.AudioEndpointVolume.MasterVolumeLevelScalar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current volume");
            return 0.0f;
        }
    }

    /// <summary>
    /// Set mute state for system audio.
    /// </summary>
    /// <param name="mute">true to mute, false to unmute</param>
    /// <returns>Result message</returns>
    public async Task<string> SetMuteStateAsync(bool mute)
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.Mute = mute;

            _logger.LogInformation($"Audio {(mute ? "muted" : "unmuted")}");
            return $"Audio {(mute ? "muted" : "unmuted")} successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set mute state");
            return "Failed to set mute state";
        }
    }

    #endregion

    #region Brightness Control

    /// <summary>
    /// Adjust screen brightness up or down.
    /// </summary>
    /// <param name="inc">true to increase brightness, false to decrease</param>
    /// <returns>Result message</returns>
    public async Task<string> SetBrightnessAsync(bool inc)
    {
        try
        {
            byte currentBrightness = await GetCurrentBrightnessAsync();

            var scope = new ManagementScope("root\\WMI");
            var query = new SelectQuery("WmiMonitorBrightnessMethods");

            using var searcher = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject mo in searcher.Get())
            {
                var inParams = mo.GetMethodParameters("WmiSetBrightness");

                uint brightness;
                if (inc)
                {
                    // Increase brightness by 20%
                    brightness = Math.Min((uint)currentBrightness + 20, 100);
                }
                else
                {
                    // Decrease brightness by 20%, minimum 20%
                    brightness = Math.Max((uint)currentBrightness - 20, 20);
                }

                inParams["Brightness"] = brightness;
                inParams["Timeout"] = 1;
                mo.InvokeMethod("WmiSetBrightness", inParams, null);
            }

            _logger.LogInformation($"Brightness adjusted: {(inc ? "increased" : "decreased")}");
            return "Screen brightness adjustment successful";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to adjust brightness");
            return "Failed to adjust brightness";
        }
    }

    /// <summary>
    /// Set screen brightness to a specific percentage.
    /// </summary>
    /// <param name="percent">Brightness percentage (0-100)</param>
    /// <returns>Result message</returns>
    public async Task<string> SetBrightnessPercentAsync(int percent)
    {
        try
        {
            var scope = new ManagementScope("root\\WMI");
            var query = new SelectQuery("WmiMonitorBrightnessMethods");

            using var searcher = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject mo in searcher.Get())
            {
                var inParams = mo.GetMethodParameters("WmiSetBrightness");

                // Ensure minimum brightness of 20%
                uint brightness = (uint)Math.Max(percent, 20);
                inParams["Brightness"] = brightness;
                inParams["Timeout"] = 1;
                mo.InvokeMethod("WmiSetBrightness", inParams, null);
            }

            _logger.LogInformation($"Brightness set to {percent}%");
            return "Screen brightness set successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set brightness percentage");
            return "Failed to set brightness";
        }
    }

    /// <summary>
    /// Get current screen brightness level.
    /// </summary>
    /// <returns>Current brightness as a byte (0-100)</returns>
    public async Task<byte> GetCurrentBrightnessAsync()
    {
        try
        {
            byte currentBrightness = 100;
            using var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                currentBrightness = (byte)queryObj["CurrentBrightness"];
                break;
            }

            return currentBrightness;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current brightness");
            return 100;
        }
    }

    #endregion

    #region Resolution Control

    /// <summary>
    /// Set screen resolution.
    /// </summary>
    /// <param name="type">Resolution type: "high", "medium", or "low"</param>
    /// <returns>Result message</returns>
    public async Task<string> SetResolutionAsync(string type)
    {
        try
        {
            var devmode = new DEVMODE();
            devmode.dmDeviceName = new string(new char[32]);
            devmode.dmFormName = new string(new char[32]);
            devmode.dmSize = (short)Marshal.SizeOf(devmode);

            if (NativeMethods.EnumDisplaySettings(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref devmode) != 0)
            {
                switch (type.ToLower())
                {
                    case "medium" or "中":
                        devmode.dmPelsWidth = 1920;
                        devmode.dmPelsHeight = 1080;
                        break;
                    case "low" or "低":
                        devmode.dmPelsWidth = 1600;
                        devmode.dmPelsHeight = 900;
                        break;
                    default: // high
                        devmode.dmPelsWidth = 2560;
                        devmode.dmPelsHeight = 1600;
                        break;
                }

                // Test the resolution change
                int result = NativeMethods.ChangeDisplaySettings(ref devmode, NativeMethods.CDS_TEST);

                if (result == NativeMethods.DISP_CHANGE_FAILED)
                {
                    return "Failed to change resolution - unsupported resolution";
                }

                // Apply the resolution change
                result = NativeMethods.ChangeDisplaySettings(ref devmode, NativeMethods.CDS_UPDATEREGISTRY);

                return result switch
                {
                    NativeMethods.DISP_CHANGE_SUCCESSFUL => "Resolution changed successfully",
                    NativeMethods.DISP_CHANGE_RESTART => "Resolution changed - restart required for full effect",
                    _ => "Failed to change resolution"
                };
            }

            return "Failed to get current display settings";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set resolution");
            return "Failed to set resolution";
        }
    }

    #endregion

    #region Native Methods and Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public static class NativeMethods
    {
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;

        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
    }

    #endregion
}