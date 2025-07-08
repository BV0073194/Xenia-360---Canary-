using System;
using System.IO;
using System.Linq;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;
using Microsoft.Maui.Storage; // Add this using statement for FileSystem

namespace Xenia_360____Canary_.Views
{
    public partial class XeniaStorePage : ContentPage
    {
        // In a real app with DI, this would be injected. Creating an instance is fine for now.
        private readonly DownloadManagerService _downloadManager = new();

        public XeniaStorePage()
        {
            InitializeComponent();
        }

        private void StoreWebView_OnNavigating(object? sender, WebNavigatingEventArgs e)
        {
            var uri = new Uri(e.Url);

            // Check if the URL is a direct link to a file from a supported host
            bool isDownloadLink = Path.HasExtension(uri.AbsolutePath) &&
                                  (uri.Host.Contains("myrient.erista.me") || (uri.Host.Contains("archive.org") && uri.AbsolutePath.Contains("/download/")));

            if (isDownloadLink)
            {
                // Cancel the navigation in the WebView since we're handling it
                e.Cancel = true;

                // Use the main thread to display the UI alert
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var fileName = Path.GetFileName(uri.AbsolutePath);
                    bool confirm = await DisplayAlert("Download Confirmation", $"Do you want to download this file?\n\n{fileName}", "Yes", "No");

                    if (confirm)
                    {
                        // --- THIS IS THE FIX ---
                        // Use the reliable FileSystem.AppDataDirectory for all paths

                        var downloadFolder = Path.Combine(FileSystem.AppDataDirectory, "Downloads");
                        var gameFolder = Path.Combine(FileSystem.AppDataDirectory, "ROMS", Path.GetFileNameWithoutExtension(fileName));

                        // Ensure the target directories exist
                        if (!Directory.Exists(downloadFolder))
                            Directory.CreateDirectory(downloadFolder);
                        if (!Directory.Exists(gameFolder))
                            Directory.CreateDirectory(gameFolder);

                        var task = new DownloadTask
                        {
                            Url = e.Url,
                            Destination = Path.Combine(downloadFolder, fileName),
                            GameFolder = gameFolder, // This now uses the correct, reliable path
                            GameTitle = Path.GetFileNameWithoutExtension(fileName) ?? "New Game"
                        };

                        _downloadManager.EnqueueDownload(task);
                        await DisplayAlert("Download Queued", $"{task.GameTitle} has been added to the download queue.", "OK");
                    }
                });
            }
        }
    }
}