using Xenia_360____Canary_.ViewModels;

namespace Xenia_360____Canary_.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
}