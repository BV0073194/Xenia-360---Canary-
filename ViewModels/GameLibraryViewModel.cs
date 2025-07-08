using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_.ViewModels
{
    public class GameLibraryViewModel : BaseViewModel
    {
        private readonly XeniaLauncherService _xeniaLauncher;
        private readonly SettingsService _settingsService;
        private readonly DownloadManagerService _downloadManager;
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

        public GameLibraryViewModel(IDispatcher dispatcher, SettingsService settingsService, XeniaLauncherService xeniaLauncher, DownloadManagerService downloadManager)
        {
            _settingsService = settingsService;
            _xeniaLauncher = xeniaLauncher;
            _downloadManager = downloadManager;
            _controllerService = new ControllerService(dispatcher); // ControllerService is trickier for DI, this is fine

            PlayGameCommand = new Command(async () => await PlayGame(), () => IsGameSelected);
            AddGameFromFileCommand = new Command(async () => await AddGameFromFile());

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

            // This now reads from the single, shared settings instance
            string? xeniaPath = _settingsService.Settings.XeniaCanaryPath;

            if (string.IsNullOrEmpty(xeniaPath) || !File.Exists(xeniaPath))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Xenia path not configured or not found. Please set it in Settings.", "OK");
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
            var fileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".zip", ".iso" } }
                });

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a game file (.zip or .iso)",
                FileTypes = fileTypes
            });

            if (result != null)
            {
                var (success, message) = await _downloadManager.ProcessLocalFileAsync(result.FullPath);
                await App.Current.MainPage.DisplayAlert(success ? "Success" : "Error", message, "OK");

                if (success)
                {
                    LoadGames();
                }
            }
        }

        private void OnHomeButtonPressed()
        {
            _xeniaLauncher.CloseXenia();
        }
    }
}