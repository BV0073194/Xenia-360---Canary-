#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xenia_360____Canary_.Models;
#if WINDOWS
// We need to interact with the native WinUI window, so we bring in these namespaces.
using Microsoft.UI.Xaml;
using WinRT.Interop;  // Required for WindowNative.GetWindowHandle
#endif

namespace Xenia_360____Canary_.Services;

public class XeniaLauncherService
{
    // P/Invoke declarations to interact with the Windows API for window management.
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

        // Ensure the launch path is not null before checking if the file exists.
        if (game.DefaultLaunchPath == null || !File.Exists(game.DefaultLaunchPath))
            throw new Exception($"Game launch file not found: {game.DefaultLaunchPath}");

        var psi = new ProcessStartInfo
        {
            FileName = xeniaExePath,
            Arguments = $"\"{game.DefaultLaunchPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _xeniaProcess = Process.Start(psi);

        // Wait a moment for the Xenia process and its main window to initialize.
        await Task.Delay(1500);

        if (_xeniaProcess == null || _xeniaProcess.HasExited)
            throw new Exception("Failed to start or find the Xenia process.");

        var hwnd = _xeniaProcess.MainWindowHandle;
        if (hwnd == IntPtr.Zero)
            throw new Exception("Failed to get Xenia's main window handle. The process may have started but the window is not available yet.");

        var parentHwnd = GetAppWindowHandle();
        if (parentHwnd == IntPtr.Zero)
            throw new Exception("Failed to get the host application's window handle.");

        // Set the MAUI app's window as the parent of the Xenia window.
        SetParent(hwnd, parentHwnd);
        // Show the Xenia window. The value '5' corresponds to SW_SHOW.
        ShowWindow(hwnd, 5);

        _xeniaProcess.EnableRaisingEvents = true;
        _xeniaProcess.Exited += (s, e) => OnXeniaClosed?.Invoke();
    }

    private IntPtr GetAppWindowHandle()
    {
        #if WINDOWS
        // FIX: Use the fully qualified name for the MAUI Application class to resolve ambiguity.
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows[0];
        if (mauiWindow == null)
            throw new InvalidOperationException("Could not find the current MAUI window.");

        // The handler's PlatformView on Windows is the native WinUI Window.
        // FIX: Cast to Microsoft.UI.Xaml.Window to resolve ambiguity.
        var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow == null)
            throw new InvalidOperationException("Failed to get the native WinUI window from the MAUI window handler.");

        // Return the handle (HWND) of the native window.
        return WindowNative.GetWindowHandle(nativeWindow);
        #else
        // Return a zero pointer on non-Windows platforms.
        return IntPtr.Zero;
        #endif
    }

    public void CloseXenia()
    {
        if (_xeniaProcess != null && !_xeniaProcess.HasExited)
        {
            _xeniaProcess.Kill();
        }
    }
}
