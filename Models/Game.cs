using System.IO;

namespace Xenia_360____Canary_.Models;

public class Game
{
    public string? TitleID { get; set; }
    public string? TitleName { get; set; }
    public string? ExecutablePath { get; set; }
    public string? ContentID { get; set; }
    public string? GameFolderPath { get; set; }
    public string? LaunchPath { get; set; }

    public string DefaultLaunchPath =>
        !string.IsNullOrEmpty(LaunchPath)
            ? LaunchPath
            : Path.Combine(GameFolderPath ?? string.Empty, ExecutablePath ?? "default.xex");

    public string DisplayTitle =>
        !string.IsNullOrEmpty(TitleName) ? TitleName : Path.GetFileName(GameFolderPath ?? string.Empty);
}
