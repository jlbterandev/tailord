using Avalonia.Collections;
using Avalonia.Threading;
using Tailord.Core;

namespace Tailord.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const int BatchSize = 200;
    private const int MaximumDisplayedLines = 10_000;

    private readonly LogFileReader _reader = new();
    private CancellationTokenSource? _readCancellation;
    private string _selectedFileName = "No logs open";
    private string _selectedFilePath = "Select a local log file to begin.";
    private string _status = "Ready for the first log";

    public string Title => TailordProduct.Name;

    public string Description => TailordProduct.Description;

    public AvaloniaList<LogEntry> Entries { get; } = [];

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

    public async Task OpenFileAsync(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        _readCancellation?.Cancel();
        CancellationTokenSource cancellation = new();
        _readCancellation = cancellation;

        SelectedFileName = Path.GetFileName(path);
        SelectedFilePath = path;
        Entries.Clear();
        Status = "Reading log...";

        try
        {
            long linesRead = await Task.Run(
                () => ReadFileAsync(path, cancellation.Token),
                cancellation.Token);

            if (ReferenceEquals(_readCancellation, cancellation))
            {
                Status = $"{linesRead:N0} lines read · {Entries.Count:N0} displayed";
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            if (ReferenceEquals(_readCancellation, cancellation))
            {
                Status = $"Cannot read log: {exception.Message}";
            }
        }
        finally
        {
            if (ReferenceEquals(_readCancellation, cancellation))
            {
                _readCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    public void ReportNonLocalFile()
    {
        Status = "The selected item is not available as a local file.";
    }

    public void CancelReading() => _readCancellation?.Cancel();

    private async Task<long> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        List<LogEntry> batch = new(BatchSize);
        long linesRead = 0;

        await foreach (string line in _reader.ReadExistingAsync(path, cancellationToken))
        {
            batch.Add(
                new LogEntry(
                    line,
                    LogTextClassifier.DetectLevel(line),
                    IsException: LogTextClassifier.ContainsException(line)));
            linesRead++;

            if (batch.Count == BatchSize)
            {
                await AddBatchAsync(batch, linesRead, cancellationToken);
                batch = new List<LogEntry>(BatchSize);
            }
        }

        if (batch.Count > 0)
        {
            await AddBatchAsync(batch, linesRead, cancellationToken);
        }

        return linesRead;
    }

    private async Task AddBatchAsync(
        IReadOnlyCollection<LogEntry> batch,
        long linesRead,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                int linesToRemove = Math.Max(
                    0,
                    Entries.Count + batch.Count - MaximumDisplayedLines);

                if (linesToRemove > 0)
                {
                    Entries.RemoveRange(0, linesToRemove);
                }

                Entries.AddRange(batch);
                Status = $"Reading log... {linesRead:N0} lines";
            });
    }
}
