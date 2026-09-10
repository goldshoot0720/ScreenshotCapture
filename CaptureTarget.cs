namespace ScreenshotCapture;

public sealed record CaptureTarget(nint Handle, string Title, string ProcessName, int Width, int Height, string WindowState)
{
    public string DisplayProcessName => $"{ProcessName} · {WindowState}";
    public string Size => $"{Width} × {Height}";
}