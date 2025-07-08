using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_.ViewModels
{
    public class GameLibraryViewModel : BaseViewModel
    {
        private readonly XeniaLauncherService _xeniaLauncher = new();
        private readonly SettingsService _settingsService = new();
        private readonly ControllerService _controllerService;

        private ObservableCollection<Game> _games = new();
        public ObservableCollection<Game> Games
        {
            get => _games;
            set => SetProperty(ref _games, value);
        }

        private Game? _selectedGame;
        public Game? SelectedGame
        {
            get => _selectedGame;
            set
            {
                SetProperty(ref _selectedGame, value);
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public bool IsGameSelected => SelectedGame != null;

        public ICommand PlayGameCommand { get; }
        public ICommand AddGameFromFileCommand { get; }

        public GameLibraryViewModel(IDispatcher dispatcher)
        {
            PlayGameCommand = new Command(async () => await PlayGame(), () => IsGameSelected);
            AddGameFromFileCommand = new Command(async () => await AddGameFromFile());
            _controllerService = new ControllerService(dispatcher);

            LoadGames();

            _controllerService.OnHomeButtonPressed += OnHomeButtonPressed;
            _xeniaLauncher.OnXeniaClosed += () => dispatcher.Dispatch(LoadGames);
        }

        void LoadGames()
        {
            GameLibraryService.Load();
            Games = new ObservableCollection<Game>(GameLibraryService.Games);
        }

        async Task PlayGame()
        {
            if (SelectedGame == null) return;

            // This logic should be expanded based on settings
            string? xeniaPath = _settingsService.Settings.XeniaCanaryPath;

            if (string.IsNullOrEmpty(xeniaPath))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Xenia path not configured. Please set it in Settings.", "OK");
                return;
            }

            try
            {
                await _xeniaLauncher.LaunchGameAsync(SelectedGame, xeniaPath);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Launch Error", ex.Message, "OK");
            }
        }

        async Task AddGameFromFile()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                // Title is no longer a valid property here
                PickerTitle = "Select a game file (.iso, .zip, default.xex)"
            });

            if (result != null)
            {
                await App.Current.MainPage.DisplayAlert("WIP", "Manual game adding is a work in progress. For now, please use the Xenia Store feature.", "OK");
            }
        }

        private void OnHomeButtonPressed()
        {
            _xeniaLauncher.CloseXenia();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Xenia", "Xenia has been closed.", "OK");
            });
        }
    }
}