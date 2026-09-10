using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ScreenshotCapture;

internal static class WindowCaptureService
{
    private const uint PwRenderFullContent = 0x00000002;

    public static Task CaptureToPngAsync(CaptureTarget target, string path) => Task.Run(() =>
    {
        using var audio = SystemAudioSilencer.MuteMasterEndpoint();
        CaptureWindow(target.Handle, path);
    });

    private static void CaptureWindow(nint window, string path)
    {
        if (!IsWindow(window) || !GetWindowRect(window, out var rect))
            throw new InvalidOperationException("選取的視窗已關閉。請重新整理後再試一次。");

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        if (width < 2 || height < 2)
            throw new InvalidOperationException("選取的視窗目前無法擷取。");

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            var deviceContext = graphics.GetHdc();
            try
            {
                if (!PrintWindow(window, deviceContext, PwRenderFullContent))
                    throw new InvalidOperationException("Windows 無法擷取這個 App 視窗。請確認它未被關閉或受保護。");
            }
            finally
            {
                graphics.ReleaseHdc(deviceContext);
            }
        }

        bitmap.Save(path, ImageFormat.Png);
    }

    [DllImport("user32.dll")] private static extern bool IsWindow(nint hWnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint hWnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool PrintWindow(nint hwnd, nint hdcBlt, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left; public int Top; public int Right; public int Bottom; }
}
