#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services
{
    public class XeniaLauncherService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private Process? _xeniaProcess;
        public event Action? OnXeniaClosed;
        private readonly ToolManagerService _toolManager = new();

        public async Task LaunchGameAsync(Game game, string xeniaExePath)
        {
            if (string.IsNullOrEmpty(xeniaExePath) || !File.Exists(xeniaExePath))
                throw new Exception("Xenia executable not found at the specified path.");

            var launchPath = game.DefaultLaunchPath;
            if (string.IsNullOrEmpty(launchPath) || !File.Exists(launchPath))
                throw new Exception($"Game launch file not found: {launchPath}");

            var psi = new ProcessStartInfo
            {
                FileName = xeniaExePath,
                Arguments = $"\"{launchPath}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            _xeniaProcess = Process.Start(psi);

            if (_xeniaProcess == null)
            {
                throw new Exception("Failed to start the Xenia process.");
            }

            _xeniaProcess.EnableRaisingEvents = true;
            _xeniaProcess.Exited += (s, e) => OnXeniaClosed?.Invoke();

            // Run window nesting and metadata scan in the background
            _ = Task.Run(() => PostLaunchOperations(game));
        }

        private async Task PostLaunchOperations(Game game)
        {
            if (_xeniaProcess == null) return;

            try
            {
                // Wait for the window to be ready and get its handle
                IntPtr xeniaHwnd = IntPtr.Zero;
                for (int i = 0; i < 20; i++) // Wait up to 10 seconds
                {
                    await Task.Delay(500);
                    _xeniaProcess.Refresh();
                    if (_xeniaProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        xeniaHwnd = _xeniaProcess.MainWindowHandle;
                        break;
                    }
                }

                if (xeniaHwnd != IntPtr.Zero)
                {
                    // --- FIX FOR WINDOW NESTING ---
                    // This needs to be run on the main UI thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var parentHwnd = GetAppWindowHandle();
                        if (parentHwnd != IntPtr.Zero)
                        {
                            SetParent(xeniaHwnd, parentHwnd);
                            const int SW_SHOWMAXIMIZED = 3; // Constant for maximizing window
                            ShowWindow(xeniaHwnd, SW_SHOWMAXIMIZED);
                        }
                    });

                    // If the game has not been scanned yet, do it now.
                    if (!game.IsMetadataScanned)
                    {
                        await ScanRunningGameMetadataAsync(game, _xeniaProcess.MainWindowTitle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Post-launch operations failed: {ex.Message}");
            }
        }

        private async Task ScanRunningGameMetadataAsync(Game game, string xeniaWindowTitle)
        {
            if (string.IsNullOrEmpty(xeniaWindowTitle)) return;

            var dumpRootPath = Path.Combine(AppContext.BaseDirectory, "X360_DUMP");
            var dumpOutputPath = Path.Combine(dumpRootPath, game.DisplayTitle.Trim());
            Directory.CreateDirectory(dumpOutputPath);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _toolManager.VfsDumpPath,
                    // --- FIX FOR VFS DUMP COMMAND ---
                    Arguments = $"--target \"{xeniaWindowTitle}\" --output \"{dumpOutputPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true, // Hide the dump window
                };

                using var proc = Process.Start(psi);
                if (proc == null) return;

                await proc.WaitForExitAsync();

                if (proc.ExitCode != 0)
                {
                    Debug.WriteLine($"xenia-vfs-dump failed for window '{xeniaWindowTitle}'.");
                    return;
                }

                // The tool dumps the content, we need to find the json file inside
                string vfsJsonPath = Path.Combine(dumpOutputPath, "vfs_output.json");
                if (File.Exists(vfsJsonPath))
                {
                    ParseVfsDumpAndUpdateGame(vfsJsonPath, game);
                    game.IsMetadataScanned = true;
                    GameLibraryService.Save(); // Save the updated game info
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during metadata scan: {ex.Message}");
            }
            finally
            {
                // --- FIX FOR CLEANUP ---
                if (Directory.Exists(dumpRootPath))
                {
                    Directory.Delete(dumpRootPath, true);
                }
            }
        }

        private void ParseVfsDumpAndUpdateGame(string jsonPath, Game game)
        {
            if (!File.Exists(jsonPath)) return;

            var json = File.ReadAllText(jsonPath);
            var obj = JObject.Parse(json);

            var gameData = obj["volume"]?["game"];
            if (gameData == null) return;

            game.TitleID = gameData["title_id"]?.ToString();
            game.TitleName = gameData["title_name"]?.ToString();
            game.ExecutablePath = gameData["executable"]?.ToString();
            game.ContentID = gameData["content_id"]?.ToString();

            string contentPath = Path.Combine(game.GameFolderPath ?? "", "Content", "0000000000000000", game.TitleID ?? "", game.ContentID ?? "");
            if (Directory.Exists(contentPath))
            {
                game.LaunchPath = Directory.GetFiles(contentPath, "*", SearchOption.AllDirectories)
                    .FirstOrDefault(f => f.EndsWith(".xex", StringComparison.OrdinalIgnoreCase));
            }
        }

        private IntPtr GetAppWindowHandle()
        {
#if WINDOWS
            var window = App.Current?.Windows[0];
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                return WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            }
#endif
            return IntPtr.Zero;
        }

        public void CloseXenia()
        {
            if (_xeniaProcess != null && !_xeniaProcess.HasExited)
            {
                _xeniaProcess.Kill();
            }
        }
    }
}