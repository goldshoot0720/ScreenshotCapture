using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenshotCapture;

internal static class WindowCatalog
{
    private const int GwlExStyle = -20;
    private const long WsExToolWindow = 0x00000080L;

    public static IReadOnlyList<CaptureTarget> GetCaptureTargets()
    {
        var ownProcessId = Environment.ProcessId;
        var foregroundWindow = GetForegroundWindow();
        var result = new List<CaptureTarget>();
        EnumWindows((window, _) =>
        {
            if (!IsWindowVisible(window) || GetWindowTextLength(window) == 0 || GetWindowThreadProcessId(window, out var processId) == 0 || processId == ownProcessId)
                return true;

            if ((GetWindowLongPtr(window, GwlExStyle).ToInt64() & WsExToolWindow) != 0 || !GetWindowRect(window, out var rect))
                return true;

            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;
            if (width < 2 || height < 2) return true;

            var title = ReadWindowText(window);
            try
            {
                var process = Process.GetProcessById((int)processId);
                var state = IsIconic(window)
                    ? "已最小化"
                    : window == foregroundWindow ? "目前前景" : "背景中";
                result.Add(new CaptureTarget(window, title, process.ProcessName, width, height, state));
            }
            catch (ArgumentException) { }
            return true;
        }, IntPtr.Zero);

        // EnumWindows returns top-level windows in z-order, keeping recently active background apps near the top.
        return result;
    }

    private static string ReadWindowText(nint window)
    {
        var builder = new StringBuilder(GetWindowTextLength(window) + 1);
        _ = GetWindowText(window, builder, builder.Capacity);
        return builder.ToString().Trim();
    }

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, nint lParam);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint hWnd);
    [DllImport("user32.dll")] private static extern bool IsIconic(nint hWnd);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(nint hWnd, StringBuilder text, int maxCount);
    [DllImport("user32.dll")] private static extern int GetWindowTextLength(nint hWnd);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint hWnd, out Rect rect);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint hWnd, int index);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left; public int Top; public int Right; public int Bottom; }
}
