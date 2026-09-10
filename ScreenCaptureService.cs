using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenshotCapture;

internal static class ScreenCaptureService
{
    public static Task CaptureDesktopToPngAsync(string path) => Task.Run(() =>
    {
        using var audio = SystemAudioSilencer.MuteMasterEndpoint();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var bounds = System.Windows.Forms.SystemInformation.VirtualScreen;
        using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        bitmap.Save(path, ImageFormat.Png);
    });
}