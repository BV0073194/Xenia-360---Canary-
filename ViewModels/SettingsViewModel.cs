using System.Windows.Input;
using Xenia_360____Canary_.Models;
using Xenia_360____Canary_.Services;

namespace Xenia_360____Canary_.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;

        private AppSettings _settings;
        public AppSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            // Create a copy for editing, so we can save it explicitly
            Settings = new AppSettings
            {
                XeniaCanaryPath = _settingsService.Settings.XeniaCanaryPath,
                XeniaNetplayPath = _settingsService.Settings.XeniaNetplayPath,
                XeniaMouselockPath = _settingsService.Settings.XeniaMouselockPath,
                DefaultXeniaVersion = _settingsService.Settings.DefaultXeniaVersion,
                ThemeColor = _settingsService.Settings.ThemeColor
            };

            SaveSettingsCommand = new Command(SaveSettings);
        }

        private void SaveSettings()
        {
            // Copy the edited settings back to the service's instance
            _settingsService.Settings.XeniaCanaryPath = Settings.XeniaCanaryPath;
            _settingsService.Settings.XeniaNetplayPath = Settings.XeniaNetplayPath;
            _settingsService.Settings.XeniaMouselockPath = Settings.XeniaMouselockPath;
            _settingsService.Settings.DefaultXeniaVersion = Settings.DefaultXeniaVersion;
            _settingsService.Settings.ThemeColor = Settings.ThemeColor;

            _settingsService.SaveSettings();
            App.Current.MainPage.DisplayAlert("Success", "Settings saved successfully.", "OK");
        }
    }
}