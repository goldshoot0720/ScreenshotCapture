using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ScreenshotCapture;

public partial class FloatingCaptureWindow : Window
{
    private MainWindow? _mainWindow;

    public FloatingCaptureWindow()
    {
        InitializeComponent();
    }

    public FloatingCaptureWindow(MainWindow mainWindow) : this()
    {
        _mainWindow = mainWindow;
    }

    private void CloseFloatingButton(object? sender, RoutedEventArgs e) => Close();

    private void ReturnToApp(object? sender, RoutedEventArgs e)
    {
        _mainWindow?.Show();
        _mainWindow?.Activate();
    }

    private async void CaptureCurrentScreen(object? sender, RoutedEventArgs e)
    {
        var sourceHandle = TryGetPlatformHandle()?.Handle ?? nint.Zero;
        var mainWasVisible = _mainWindow?.IsVisible == true;
        var path = CaptureFolder.CreateScreenshotPath();

        try
        {
            Hide();
            if (mainWasVisible) _mainWindow!.Hide();
            await Task.Delay(150);
            await ScreenCaptureService.CaptureDesktopToPngAsync(path);
            _mainWindow?.SetStatus("已儲存全螢幕畫面，且系統音量已還原。");
        }
        catch (Exception exception)
        {
            _mainWindow?.SetStatus($"無法擷取目前畫面：{exception.Message}");
        }
        finally
        {
            if (mainWasVisible) _mainWindow!.Show();
            Show();
        }
    }
}