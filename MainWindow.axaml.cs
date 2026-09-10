using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ScreenshotCapture;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private CaptureTarget? _selectedWindow;
    private string _status = "請從清單選取要擷取的 App。";
    private string? _lastCaptureDirectory;

    public ObservableCollection<CaptureTarget> Windows { get; } = [];

    public CaptureTarget? SelectedWindow
    {
        get => _selectedWindow;
        set { _selectedWindow = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public bool CanOpenCaptureFolder => !string.IsNullOrWhiteSpace(_lastCaptureDirectory) && Directory.Exists(_lastCaptureDirectory);

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadWindows();
    }

    private void RefreshWindows(object? sender, RoutedEventArgs e) => LoadWindows();

    private void LoadWindows()
    {
        var selection = SelectedWindow?.Handle;
        Windows.Clear();
        foreach (var target in WindowCatalog.GetCaptureTargets())
            Windows.Add(target);

        SelectedWindow = Windows.FirstOrDefault(x => x.Handle == selection) ?? Windows.FirstOrDefault();
        Status = Windows.Count == 0 ? "找不到可擷取的外部應用程式視窗。請開啟目標 App 後重新整理。" : $"找到 {Windows.Count} 個可擷取視窗。";
    }

    private async void CaptureSelectedWindow(object? sender, RoutedEventArgs e)
    {
        if (SelectedWindow is null)
        {
            Status = "請先選取要擷取的 App。";
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "儲存螢幕擷取畫面",
            SuggestedFileName = $"{SanitizeFileName(SelectedWindow.Title)}-{DateTime.Now:yyyyMMdd-HHmmss}.png",
            FileTypeChoices = [new FilePickerFileType("PNG 圖片") { Patterns = ["*.png"] }]
        });
        if (file is null) return;

        try
        {
            Status = "正在擷取… 系統音量會在完成後還原。";
            await WindowCaptureService.CaptureToPngAsync(SelectedWindow, file.Path.LocalPath);
            _lastCaptureDirectory = Path.GetDirectoryName(file.Path.LocalPath);
            OnPropertyChanged(nameof(CanOpenCaptureFolder));
            Status = "已儲存擷取畫面，且系統音量已還原。";
        }
        catch (Exception exception)
        {
            Status = $"無法擷取：{exception.Message} 系統音量已還原。";
        }
    }

    private void OpenCaptureFolder(object? sender, RoutedEventArgs e)
    {
        if (!CanOpenCaptureFolder) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _lastCaptureDirectory!,
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            Status = $"無法開啟截圖資料夾：{exception.Message}";
        }
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim().TrimEnd('.') switch
        {
            "" => "screenshot",
            var result => result.Length > 70 ? result[..70] : result
        };
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
