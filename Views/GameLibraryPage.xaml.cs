using Xenia_360____Canary_.ViewModels;

namespace Xenia_360____Canary_.Views
{
    public partial class GameLibraryPage : ContentPage
    {
        public GameLibraryPage(GameLibraryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}