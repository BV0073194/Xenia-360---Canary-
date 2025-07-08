using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_;

public partial class InstallQueuePage : ContentPage
{
    private readonly DownloadManagerService _downloadManager = new();
    public ObservableCollection<DownloadTask> InstallQueue { get; set; } = new();

    public InstallQueuePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void Pause_Clicked(object sender, System.EventArgs e)
    {
        var btn = sender as Button;
        var task = btn?.BindingContext as DownloadTask;
        if (task != null)
        {
            task.IsPaused = !task.IsPaused;
            btn.Text = task.IsPaused ? "Resume" : "Pause";

            // TODO: Implement pause/resume logic in DownloadManagerService
        }
    }

    private void Remove_Clicked(object sender, System.EventArgs e)
    {
        var btn = sender as Button;
        var task = btn?.BindingContext as DownloadTask;
        if (task != null)
        {
            InstallQueue.Remove(task);
            _downloadManager.RemoveFromQueue(task);
        }
    }
}
