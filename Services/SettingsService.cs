using System.IO;
using System.Text.Json;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;
    public AppSettings Settings { get; private set; }

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        Settings = LoadSettings();
    }

    private AppSettings LoadSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        return new AppSettings();
    }

    public void SaveSettings()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }
}