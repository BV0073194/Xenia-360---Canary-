using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_;

public partial class MainPage : ContentPage
{
    private readonly XeniaLauncherService _xeniaLauncherService = new();
    private readonly DownloadManagerService _downloadManagerService = new();
    private readonly ControllerService _controllerService;

    public ObservableCollection<Game> Games { get; set; } = new();

    private Game? _selectedGame;

    private string _xeniaCanaryPath = @"C:\Path\To\Xenia\xenia-canary.exe";

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Pass the Dispatcher of this Page to ControllerService
        _controllerService = new ControllerService(Dispatcher);

        LoadGames();

        _downloadManagerService.DownloadCompleted += OnDownloadCompleted;

        _controllerService.OnHomeButtonPressed += () =>
        {
            _xeniaLauncherService.CloseXenia();
            DisplayAlert("Controller", "Home button pressed, returning to dashboard.", "OK");
        };
    }

    private void LoadGames()
    {
        GameLibraryService.Load();
        Games = new ObservableCollection<Game>(GameLibraryService.Games);
        GameLibrary.ItemsSource = Games;
    }

    private void GameLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedGame = e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] as Game : null;
    }

    private async void PlayButton_Clicked(object sender, EventArgs e)
    {
        if (_selectedGame == null)
        {
            await DisplayAlert("Error", "No game selected.", "OK");
            return;
        }

        try
        {
            await _xeniaLauncherService.LaunchGameAsync(_selectedGame, _xeniaCanaryPath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to launch game: {ex.Message}", "OK");
        }
    }

    private async void SettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }

    private async void DownloadButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DownloadPage());
    }

    private async void InstallQueueButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InstallQueuePage());
    }

    private void OnDownloadCompleted(DownloadTask task, bool success)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (success)
            {
                await DisplayAlert("Download Complete", $"Game \"{task.GameTitle}\" has finished installing.", "OK");
                LoadGames();
            }
            else
            {
                await DisplayAlert("Download Failed", $"Failed to download \"{task.GameTitle}\".", "OK");
            }
        });
    }
}
