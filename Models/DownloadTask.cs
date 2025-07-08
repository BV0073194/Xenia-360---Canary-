namespace Xenia_360____Canary_.Models;

public class DownloadTask
{
    public string Url { get; set; }
    public string Destination { get; set; }
    public string GameFolder { get; set; }
    public string GameTitle { get; set; }
    public bool IsPaused { get; set; }
}
