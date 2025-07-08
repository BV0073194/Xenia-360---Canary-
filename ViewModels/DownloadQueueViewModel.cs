using System.Collections.ObjectModel;
using Xenia_360____Canary_.Models; // Ensure this namespace is correct

namespace Xenia_360____Canary_.ViewModels; // Ensure this namespace is correct

public class DownloadQueueViewModel : BaseViewModel
{
    // The "static" keyword has been removed from the line below.
    public ObservableCollection<DownloadTask> DownloadQueue { get; set; } = new();

    public DownloadQueueViewModel()
    {
        // In a more advanced version, you would get this data from a shared service
        // instead of creating a new list each time.
    }
}