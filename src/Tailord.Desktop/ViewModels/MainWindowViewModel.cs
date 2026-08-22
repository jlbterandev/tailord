using Tailord.Core;

namespace Tailord.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private string _selectedFileName = "No logs open";
    private string _selectedFilePath = "Select a local log file to begin.";
    private string _status = "Ready for the first log";

    public string Title => TailordProduct.Name;

    public string Description => TailordProduct.Description;

    public string SelectedFileName
    {
        get => _selectedFileName;
        private set => SetProperty(ref _selectedFileName, value);
    }

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        private set => SetProperty(ref _selectedFilePath, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public void SelectFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        SelectedFileName = Path.GetFileName(path);
        SelectedFilePath = path;
        Status = "Log selected. Reading will be added in the next increment.";
    }

    public void ReportNonLocalFile()
    {
        Status = "The selected item is not available as a local file.";
    }
}
