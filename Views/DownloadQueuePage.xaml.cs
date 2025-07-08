using Xenia_360____Canary_.ViewModels;

namespace Xenia_360____Canary_.Views;

public partial class DownloadQueuePage : ContentPage
{
    public DownloadQueuePage()
    {
        InitializeComponent();
        BindingContext = new DownloadQueueViewModel();
    }
}