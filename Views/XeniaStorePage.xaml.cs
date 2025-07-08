using System.IO;
using Xenia_360____Canary_.Models; // Make sure your namespace is correct after renaming the project
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_.Views;

public partial class XeniaStorePage : ContentPage
{
    // You should use dependency injection here in the future,
    // but creating an instance for now is fine.
    private readonly DownloadManagerService _downloadManager = new();

    public XeniaStorePage()
    {
        InitializeComponent();
    }

    private void StoreWebView_OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var uri = new Uri(e.Url);

        // --- THIS IS THE FIX ---
        // We now check that the URL path actually has a file extension (like .zip or .iso).
        // This stops it from triggering on folder pages.
        bool isMyrientDownload = uri.Host.Contains("myrient.erista.me") && Path.HasExtension(uri.AbsolutePath);
        bool isArchiveOrgDownload = uri.Host.Contains("archive.org") && uri.AbsolutePath.Contains("/download/") && Path.HasExtension(uri.AbsolutePath);

        if (isMyrientDownload || isArchiveOrgDownload)
        {
            // Cancel the navigation in the WebView since we're handling it.
            e.Cancel = true;

            // Use the main thread to display the UI alert.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool confirm = await DisplayAlert("Download Confirmation", $"Do you want to download this file?\n\n{Path.GetFileName(uri.AbsolutePath)}", "Yes", "No");
                if (confirm)
                {
                    var fileName = Path.GetFileName(uri.AbsolutePath);
                    var downloadFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");
                    var gameFolder = Path.Combine(AppContext.BaseDirectory, "ROMS", Path.GetFileNameWithoutExtension(fileName));

                    var task = new DownloadTask
                    {
                        Url = e.Url,
                        Destination = Path.Combine(downloadFolder, fileName),
                        GameFolder = gameFolder,
                        GameTitle = Path.GetFileNameWithoutExtension(fileName) ?? "New Game"
                    };

                    _downloadManager.EnqueueDownload(task);
                    await DisplayAlert("Download Queued", $"{task.GameTitle} has been added to the download queue.", "OK");
                }
            });
        }
    }
}