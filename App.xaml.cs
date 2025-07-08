#nullable enable
using Microsoft.Maui.Controls;

namespace Xenia_360____Canary_;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        // To enable navigation using PushAsync, the root page must be a NavigationPage.
        // We wrap the MainPage inside a NavigationPage here.
        return new Window(new NavigationPage(new MainPage()));
    }
}
