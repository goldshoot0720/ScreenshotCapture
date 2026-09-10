namespace ScreenshotCapture;

public sealed record CaptureTarget(nint Handle, string Title, string ProcessName, int Width, int Height)
{
    public string Size => $"{Width} × {Height}";
}
