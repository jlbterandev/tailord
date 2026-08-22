using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Tailord.Desktop.ViewModels;

namespace Tailord.Desktop.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private async void OpenLog_Click(object? sender, RoutedEventArgs eventArgs)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open log file",
                AllowMultiple = false,
            });

        if (files.Count == 0 || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        string? path = files[0].TryGetLocalPath();

        if (path is null)
        {
            viewModel.ReportNonLocalFile();
            return;
        }

        await viewModel.OpenFileAsync(path);
    }

    protected override void OnClosed(EventArgs eventArgs)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CancelReading();
        }

        base.OnClosed(eventArgs);
    }
}
