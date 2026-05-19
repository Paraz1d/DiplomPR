using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string title = string.Empty;
    public SnackbarMessageQueue MessageQueue { get; } = new(TimeSpan.FromSeconds(3));
    protected void Notify(string msg) => MessageQueue.Enqueue(msg);
    protected void NotifyError(string msg) => MessageQueue.Enqueue("⚠ " + msg);
}
