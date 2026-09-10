namespace ScreenshotCapture;

internal static class CaptureFolder
{
    public static string DefaultPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "ScreenshotCapture");

    public static string CreateScreenshotPath() => Path.Combine(DefaultPath, $"screen-{DateTime.Now:yyyyMMdd-HHmmss}.png");
}