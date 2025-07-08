namespace Xenia_360____Canary_.Models;

public class AppSettings
{
    public string? XeniaCanaryPath { get; set; }
    public string? XeniaNetplayPath { get; set; }
    public string? XeniaMouselockPath { get; set; }
    public string? DefaultXeniaVersion { get; set; } = "Canary"; // "Canary", "Netplay", "Mouselock"
    public string? ThemeColor { get; set; } = "#FF0078D7"; // Default theme color
}