using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xenia_360____Canary_.Models;
#if WINDOWS
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;
using WinRT.Interop;  // for WindowNative.GetWindowHandle
#endif

namespace Xenia_360____Canary_.Services;

public class XeniaLauncherService
{
    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private Process? _xeniaProcess;

    public event Action? OnXeniaClosed;

    public async Task LaunchGameAsync(Game game, string xeniaExePath)
    {
        if (string.IsNullOrEmpty(xeniaExePath))
            throw new Exception("Xenia executable path is not set.");

        if (!File.Exists(xeniaExePath))
            throw new Exception("Xenia executable not found.");

        if (!File.Exists(game.DefaultLaunchPath))
            throw new Exception($"Game launch file not found: {game.DefaultLaunchPath}");

        var psi = new ProcessStartInfo
        {
            FileName = xeniaExePath,
            Arguments = $"\"{game.DefaultLaunchPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _xeniaProcess = Process.Start(psi);

        await Task.Delay(1500); // Give time for Xenia window to initialize

        if (_xeniaProcess == null)
            throw new Exception("Failed to start Xenia process.");

        var hwnd = _xeniaProcess.MainWindowHandle;
        if (hwnd == IntPtr.Zero)
            throw new Exception("Failed to get Xenia window handle.");

        var parentHwnd = GetAppWindowHandle();
        if (parentHwnd == IntPtr.Zero)
            throw new Exception("Failed to get host app window handle.");

        SetParent(hwnd, parentHwnd);
        ShowWindow(hwnd, 5); // SW_SHOW

        _xeniaProcess.EnableRaisingEvents = true;
        _xeniaProcess.Exited += (s, e) => OnXeniaClosed?.Invoke();
    }

    private IntPtr GetAppWindowHandle()
    {
        #if WINDOWS
        var mauiWindow = Application.Current?.Windows[0];
        if (mauiWindow == null)
            throw new InvalidOperationException("No current MAUI window found.");

        var nativeWindow = mauiWindow.Handler?.PlatformView as Window;
        if (nativeWindow == null)
            throw new InvalidOperationException("Failed to get native window.");

        return WindowNative.GetWindowHandle(nativeWindow);
        #else
        return IntPtr.Zero;
        #endif
    }

    public void CloseXenia()
    {
        if (_xeniaProcess != null && !_xeniaProcess.HasExited)
            _xeniaProcess.Kill();
    }
}
