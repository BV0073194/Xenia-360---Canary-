using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_;

public partial class DownloadPage : ContentPage
{
    private readonly DownloadManagerService _downloadManager = new();
    public ObservableCollection<DownloadTask> Downloads { get; set; } = new();

    public DownloadPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void AddDownload_Clicked(object sender, System.EventArgs e)
    {
        var url = DownloadUrlEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            DisplayAlert("Invalid URL", "Please enter a valid download URL.", "OK");
            return;
        }

        var fileName = System.IO.Path.GetFileName(url);
        var destDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Downloads");
        if (!System.IO.Directory.Exists(destDir))
            System.IO.Directory.CreateDirectory(destDir);

        var gameFolder = System.IO.Path.Combine(AppContext.BaseDirectory, "ROMS", System.IO.Path.GetFileNameWithoutExtension(fileName));
        if (!System.IO.Directory.Exists(gameFolder))
            System.IO.Directory.CreateDirectory(gameFolder);

        var task = new DownloadTask
        {
            Url = url,
            Destination = System.IO.Path.Combine(destDir, fileName),
            GameFolder = gameFolder,
            GameTitle = System.IO.Path.GetFileNameWithoutExtension(fileName)
        };

        Downloads.Add(task);
        _downloadManager.EnqueueDownload(task);
    }
}
