using Tailord.Core;

namespace Tailord.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public string Title => TailordProduct.Name;

    public string Description => TailordProduct.Description;

    public string Status => "Ready for the first log";
}

