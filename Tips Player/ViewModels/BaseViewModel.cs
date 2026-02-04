using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.ViewModels;

public partial class BaseViewModel : ObservableObject, IDisposable
{
    private bool _disposed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Releases all resources used by this ViewModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources in derived classes
        }

        _disposed = true;
    }

    /// <summary>
    /// Gets whether this ViewModel has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;
}
