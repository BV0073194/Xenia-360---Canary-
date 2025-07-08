#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Common;
using Xenia_360____Canary_.Models;
using Microsoft.Maui.Storage; // Required for FileSystem.AppDataDirectory

namespace Xenia_360____Canary_.Services
{
    public class DownloadManagerService
    {
        private readonly Queue<DownloadTask> _queue = new();
        private bool _isDownloading = false;
        public event Action<DownloadTask, bool, string>? DownloadCompleted;
        private readonly ToolManagerService _toolManager = new();

        public void EnqueueDownload(DownloadTask task)
        {
            _queue.Enqueue(task);
            if (!_isDownloading)
                _ = StartNextDownloadAsync();
        }

        public void RemoveFromQueue(DownloadTask task)
        {
            var itemsToKeep = _queue.Where(t => t.Url != task.Url).ToList();
            _queue.Clear();
            foreach (var item in itemsToKeep)
            {
                _queue.Enqueue(item);
            }
        }

        public async Task<(bool Success, string Message)> ProcessLocalFileAsync(string filePath)
        {
            try
            {
                var gameTitle = Path.GetFileNameWithoutExtension(filePath);

                // Use the reliable MAUI AppDataDirectory to build the correct ROMS path
                var romsRoot = Path.Combine(FileSystem.AppDataDirectory, "ROMS");
                var baseGameFolder = Path.Combine(romsRoot, gameTitle);

                if (!Directory.Exists(baseGameFolder))
                    Directory.CreateDirectory(baseGameFolder);

                await ExtractIfNeededAsync(filePath, baseGameFolder);

                var finalGameFolder = HandlePossibleNestedFolder(baseGameFolder);

                var game = new Game { GameFolderPath = finalGameFolder, IsMetadataScanned = false };
                GameLibraryService.AddGame(game);

                return (true, $"{gameTitle} was added to your library. Full details will be scanned on first launch.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to process file: {ex.Message}");
            }
        }

        private string HandlePossibleNestedFolder(string parentFolder)
        {
            var subDirectories = Directory.GetDirectories(parentFolder);
            var filesInParent = Directory.GetFiles(parentFolder);

            if (subDirectories.Length == 1 && filesInParent.Length == 0)
            {
                string nestedFolder = subDirectories[0];
                string newGameTitle = new DirectoryInfo(nestedFolder).Name;

                string finalDestination = Path.Combine(FileSystem.AppDataDirectory, "ROMS", newGameTitle);

                if (string.Compare(nestedFolder, finalDestination, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (Directory.Exists(finalDestination))
                    {
                        Directory.Delete(finalDestination, true);
                    }
                    Directory.Move(nestedFolder, finalDestination);
                }

                if (Directory.Exists(parentFolder) && string.Compare(parentFolder, finalDestination, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    Directory.Delete(parentFolder, true);
                }

                return finalDestination;
            }

            return parentFolder;
        }

        public async Task StartNextDownloadAsync()
        {
            if (_queue.Count == 0) { _isDownloading = false; return; }

            _isDownloading = true;
            var task = _queue.Dequeue();

            try
            {
                await DownloadFileAsync(task);
                // First, extract the game into its folder
                await ExtractIfNeededAsync(task.Destination, task.GameFolder);
                // Then, handle any nested folders created during extraction
                var finalGameFolder = HandlePossibleNestedFolder(task.GameFolder);

                var game = new Game { GameFolderPath = finalGameFolder, IsMetadataScanned = false };
                GameLibraryService.AddGame(game);

                DownloadCompleted?.Invoke(task, true, "Download successful! Game will be scanned on first launch.");
            }
            catch (Exception ex)
            {
                DownloadCompleted?.Invoke(task, false, ex.Message);
            }
            finally
            {
                _isDownloading = false;
                _ = StartNextDownloadAsync();
            }
        }

        private async Task DownloadFileAsync(DownloadTask task)
        {
            var tcs = new TaskCompletionSource<bool>();
            var psi = new ProcessStartInfo
            {
                FileName = _toolManager.Aria2Path,
                Arguments = $"--dir=\"{Path.GetDirectoryName(task.Destination)}\" --out=\"{Path.GetFileName(task.Destination)}\" -x 16 -s 16 --summary-interval=1 \"{task.Url}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            proc.Exited += (s, e) => { tcs.SetResult(proc.ExitCode == 0); proc.Dispose(); };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await tcs.Task;
        }

        private async Task ExtractIfNeededAsync(string filePath, string extractFolder)
        {
            // This is the method that executes the command you requested.
            // 'extractFolder' now correctly points to the directory inside ROMS.

            if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = ArchiveFactory.Open(filePath);
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    entry.WriteToDirectory(extractFolder, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                }
            }
            else if (filePath.EndsWith(".iso", StringComparison.OrdinalIgnoreCase))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _toolManager.ExtractXisoPath,
                    // The "-d" argument now uses the correct, reliable path.
                    Arguments = $"-x \"{filePath}\" -d \"{extractFolder}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false, // Keep this visible for debugging
                };
                using var proc = Process.Start(psi);
                if (proc != null) await proc.WaitForExitAsync();
            }
        }
    }
}