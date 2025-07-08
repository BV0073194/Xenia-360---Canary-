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
        return new Window(new MainPage());
    }
}