using System.IO;
using System.Text.Json.Serialization; // Using System.Text.Json for consistency
using Tomlyn.Model;

namespace Xenia_360____Canary_.Models;

public class Game
{
    public string? TitleID { get; set; }
    public string? TitleName { get; set; }
    public string? ExecutablePath { get; set; }
    public string? ContentID { get; set; }
    public string? GameFolderPath { get; set; }
    public string? LaunchPath { get; set; }

    // --- NEW PROPERTY ---
    public bool IsMetadataScanned { get; set; } = false;

    // Properties for per-game settings
    public string? CustomXeniaPath { get; set; }
    public TomlTable? GameSpecificConfig { get; set; }

    [JsonIgnore]
    public string DefaultLaunchPath =>
        !string.IsNullOrEmpty(LaunchPath)
            ? LaunchPath
            : Path.Combine(GameFolderPath ?? string.Empty, ExecutablePath ?? "default.xex");

    [JsonIgnore]
    public string DisplayTitle =>
        !string.IsNullOrEmpty(TitleName) ? TitleName : Path.GetFileNameWithoutExtension(GameFolderPath ?? string.Empty);

    [JsonIgnore]
    public string CoverArtPath => Path.Combine(GameFolderPath ?? string.Empty, "cover.png");
}