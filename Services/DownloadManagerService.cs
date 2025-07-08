#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services;

public class DownloadManagerService
{
    private readonly Queue<DownloadTask> _queue = new();
    private bool _isDownloading = false;

    public event Action<DownloadTask, bool>? DownloadCompleted;
    public event Action<DownloadTask, double>? DownloadProgress;

    private readonly ToolManagerService _toolManager;

    public DownloadManagerService()
    {
        _toolManager = new ToolManagerService();
    }

    public void EnqueueDownload(DownloadTask task)
    {
        _queue.Enqueue(task);
        if (!_isDownloading)
            _ = StartNextDownloadAsync();
    }

    public void RemoveFromQueue(DownloadTask task)
    {
        var itemsToKeep = _queue.Where(t => t != task).ToList();
        _queue.Clear();
        foreach (var item in itemsToKeep)
        {
            _queue.Enqueue(item);
        }
    }

    public async Task StartNextDownloadAsync()
    {
        if (_queue.Count == 0)
        {
            _isDownloading = false;
            return;
        }

        _isDownloading = true;
        var task = _queue.Dequeue();

        try
        {
            bool success = await DownloadFileAsync(task);
            if (!success)
                throw new Exception("aria2c download failed. Check the console for more details.");

            await ExtractIfNeededAsync(task.Destination, task.GameFolder);
            await GenerateGameMetadata(task.GameFolder);

            DownloadCompleted?.Invoke(task, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Download failed for {task.GameTitle}: {ex.Message}");
            DownloadCompleted?.Invoke(task, false);
        }

        await StartNextDownloadAsync();
    }

    private Task<bool> DownloadFileAsync(DownloadTask task)
    {
        var tcs = new TaskCompletionSource<bool>();

        var psi = new ProcessStartInfo
        {
            FileName = _toolManager.Aria2Path,
            Arguments = $"--dir=\"{Path.GetDirectoryName(task.Destination)}\" " +
                        $"--out=\"{Path.GetFileName(task.Destination)}\" " +
                        "-x 16 -s 16 " +
                        "--user-agent=\"Mozilla/5.0 (Windows NT 10.0; Win64; x64)\" " +
                        "--referer=\"https://archive.org/\" " +
                        "--summary-interval=1 " + // Get progress updates every second
                        $"\"{task.Url}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        // FIX: Added logic to parse standard output for progress updates.
        proc.OutputDataReceived += (sender, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;

            // Regex to find the percentage from aria2c's output, e.g., (99%)
            var match = Regex.Match(args.Data, @"\(([^)]+%)\)");
            if (match.Success)
            {
                var percentageString = match.Groups[1].Value.Replace("%", "");
                if (double.TryParse(percentageString, NumberStyles.Any, CultureInfo.InvariantCulture, out double percentage))
                {
                    // Invoke the progress event to update the UI
                    DownloadProgress?.Invoke(task, percentage);
                }
            }
        };

        proc.Exited += (s, e) =>
        {
            tcs.SetResult(proc.ExitCode == 0);
            proc.Dispose();
        };

        proc.Start();
        proc.BeginOutputReadLine(); // Start reading the output asynchronously
        proc.BeginErrorReadLine();

        return tcs.Task;
    }

    private async Task ExtractIfNeededAsync(string filePath, string extractFolder)
    {
        if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ArchiveFactory.Open(filePath);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                    entry.WriteToDirectory(extractFolder, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
            }
        }
        else if (filePath.EndsWith(".iso", StringComparison.OrdinalIgnoreCase))
        {
            var psi = new ProcessStartInfo
            {
                FileName = _toolManager.ExtractXisoPath,
                Arguments = $"-x \"{filePath}\" -d \"{extractFolder}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var proc = Process.Start(psi);
            if (proc != null)
                await proc.WaitForExitAsync();
        }
    }

    private async Task GenerateGameMetadata(string gameFolder)
    {
        string vfsOutputPath = Path.Combine(gameFolder, "vfs_output.json");

        var psi = new ProcessStartInfo
        {
            FileName = _toolManager.VfsDumpPath,
            Arguments = $"\"{gameFolder}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            throw new Exception("Failed to start xenia-vfs-dump.exe process.");
        }

        string output = await proc.StandardOutput.ReadToEndAsync();
        string error = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            throw new Exception($"xenia-vfs-dump.exe failed with exit code {proc.ExitCode}: {error}");
        }

        await File.WriteAllTextAsync(vfsOutputPath, output);

        var game = new Game { GameFolderPath = gameFolder };
        ParseVfsDump(vfsOutputPath, game);

        GameLibraryService.AddGame(game);
    }

    private void ParseVfsDump(string jsonPath, Game game)
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
        else
        {
            game.LaunchPath = Path.Combine(game.GameFolderPath ?? "", game.ExecutablePath ?? "default.xex");
        }
    }
}
