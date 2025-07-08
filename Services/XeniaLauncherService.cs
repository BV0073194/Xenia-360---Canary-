#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services;

public class XeniaLauncherService
{
    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private Process? _xeniaProcess;
    private readonly ConfigService _configService = new();
    private string? _activeConfigPath;

    public event Action? OnXeniaClosed;

    public async Task LaunchGameAsync(Game game, string xeniaExePath)
    {
        if (string.IsNullOrEmpty(xeniaExePath) || !File.Exists(xeniaExePath))
            throw new Exception("Xenia executable not found at the specified path.");

        if (game.DefaultLaunchPath == null || !File.Exists(game.DefaultLaunchPath))
            throw new Exception($"Game launch file not found: {game.DefaultLaunchPath}");

        _activeConfigPath = Path.Combine(Path.GetDirectoryName(xeniaExePath)!, "xenia-canary.config.toml");

        // Handle per-game configuration
        if (game.GameSpecificConfig != null)
        {
            _configService.BackupConfig(_activeConfigPath);
            _configService.SaveConfig(_activeConfigPath, game.GameSpecificConfig);
        }

        var psi = new ProcessStartInfo
        {
            FileName = xeniaExePath,
            Arguments = $"\"{game.DefaultLaunchPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _xeniaProcess = Process.Start(psi);

        if (_xeniaProcess != null)
        {
            _xeniaProcess.EnableRaisingEvents = true;
            _xeniaProcess.Exited += XeniaProcess_Exited;
            await NestXeniaWindow();
        }
        else
        {
            throw new Exception("Failed to start the Xenia process.");
        }
    }

    private async Task NestXeniaWindow()
    {
        // Give Xenia time to initialize its main window
        await Task.Delay(2000);

        if (_xeniaProcess == null || _xeniaProcess.HasExited) return;

        var hwnd = _xeniaProcess.MainWindowHandle;
        if (hwnd == IntPtr.Zero)
        {
            // Retry logic can be added here if needed
            return;
        }

#if WINDOWS
        var parentHwnd = GetAppWindowHandle();
        if (parentHwnd != IntPtr.Zero)
        {
            SetParent(hwnd, parentHwnd);
            const int SW_SHOWMAXIMIZED = 3;
            ShowWindow(hwnd, SW_SHOWMAXIMIZED);
        }
#endif
    }

    private void XeniaProcess_Exited(object? sender, EventArgs e)
    {
        // Restore original config if a backup was made
        if (_activeConfigPath != null)
        {
            _configService.RestoreBackup(_activeConfigPath);
            _activeConfigPath = null;
        }
        OnXeniaClosed?.Invoke();
    }

    public void CloseXenia()
    {
        if (_xeniaProcess != null && !_xeniaProcess.HasExited)
        {
            _xeniaProcess.Kill(); // Forcefully close
        }
    }

#if WINDOWS
    private IntPtr GetAppWindowHandle()
    {
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows[0];
        if (mauiWindow?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
        }
        return IntPtr.Zero;
    }
#else
    private IntPtr GetAppWindowHandle() => IntPtr.Zero;
#endif
}