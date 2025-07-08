namespace Xenia_360____Canary_
{
    public partial class App : Application
    {
        public App()
        {
            // Remove or comment out InitializeComponent() if you do not have an App.xaml file with XAML content.
            // If you do have App.xaml, ensure its Build Action is set to "MauiXaml" and that it defines the correct class.
            // InitializeComponent();

            MainPage = new AppShell();
        }
    }
}