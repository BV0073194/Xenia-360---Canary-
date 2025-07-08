using Xenia_360____Canary_.Views;

namespace Xenia_360____Canary_
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for all pages that will be navigated to
            Routing.RegisterRoute(nameof(GameLibraryPage), typeof(GameLibraryPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(DownloadQueuePage), typeof(DownloadQueuePage));
            Routing.RegisterRoute(nameof(XeniaStorePage), typeof(XeniaStorePage));
        }
    }
}