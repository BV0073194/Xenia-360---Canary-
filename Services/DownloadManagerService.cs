using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    public event Action<DownloadTask, bool> DownloadCompleted;
    public event Action<DownloadTask, double> DownloadProgress;

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
        var tempQueue = new Queue<DownloadTask>();
        while (_queue.Count > 0)
        {
            var t = _queue.Dequeue();
            if (t != task)
                tempQueue.Enqueue(t);
        }
        while (tempQueue.Count > 0)
            _queue.Enqueue(tempQueue.Dequeue());
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
                throw new Exception("aria2c download failed");

            await ExtractIfNeededAsync(task.Destination, task.GameFolder);
            await GenerateGameMetadata(task.GameFolder);

            DownloadCompleted?.Invoke(task, true);
        }
        catch
        {
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
            Arguments = $"--dir=\"{Path.GetDirectoryName(task.Destination)}\" --out=\"{Path.GetFileName(task.Destination)}\" \"{task.Url}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var proc = new Process { StartInfo = psi };
        proc.EnableRaisingEvents = true;

        proc.Exited += (s, e) =>
        {
            tcs.SetResult(proc.ExitCode == 0);
            proc.Dispose();
        };

        proc.Start();

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
                Arguments = $"\"{filePath}\" \"{extractFolder}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var proc = Process.Start(psi);
            await proc.WaitForExitAsync();
        }
    }

    private async Task GenerateGameMetadata(string gameFolder)
    {
        string vfsOutputPath = Path.Combine(gameFolder, "vfs_output.json");

        var psi = new ProcessStartInfo
        {
            FileName = _toolManager.VfsDumpPath,
            Arguments = $"\"{gameFolder}\" > \"{vfsOutputPath}\"",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        var proc = Process.Start(psi);
        await proc.WaitForExitAsync();

        var game = new Game { GameFolderPath = gameFolder };
        ParseVfsDump(vfsOutputPath, game);

        // Add to your Game Library
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

        string contentPath = Path.Combine(game.GameFolderPath, "Content", "0000000000000000", game.TitleID ?? "", game.ContentID ?? "");

        if (Directory.Exists(contentPath))
        {
            // GOD-style game structure
            game.LaunchPath = Directory.GetFiles(contentPath, "*", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.EndsWith(".xex", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // Use default.xex
            game.LaunchPath = Path.Combine(game.GameFolderPath, game.ExecutablePath ?? "default.xex");
        }
    }
}
