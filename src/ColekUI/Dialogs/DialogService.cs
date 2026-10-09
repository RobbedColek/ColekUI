using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ColekUI.Controls;

namespace ColekUI.Dialogs;

public sealed class DialogService : IDialogService
{
    public Task<TResult?> ShowAsync<TResult>(DialogWindow dialog, Window? owner = null) =>
        ShowCoreAsync<TResult>(dialog, owner);

    public Task ShowAsync(DialogWindow dialog, Window? owner = null) =>
        ShowCoreAsync<object>(dialog, owner);

    public async Task<bool> ConfirmAsync(string title, string message, string? yesText = null, string? noText = null, Window? owner = null) =>
        await ShowCoreAsync<bool?>(new ConfirmDialog(title, message, yesText, noText), owner) ?? false;

    public Task ShowErrorAsync(string title, string message, string? okText = null, Window? owner = null) =>
        ShowCoreAsync<object>(new ErrorDialog(title, message, okText), owner);

    public Task<int?> ChooseAsync(string title, string message, IReadOnlyList<string> options, string? cancelText = null, Window? owner = null) =>
        ShowCoreAsync<int?>(new ChoiceDialog(title, message, options, cancelText), owner);

    internal static async Task<TResult?> ShowCoreAsync<TResult>(DialogWindow dialog, Window? owner)
    {
        owner ??= ResolveOwner();

        if (owner is null || !owner.IsVisible)
        {
            var closed = new TaskCompletionSource();
            dialog.Closed += (_, _) => closed.TrySetResult();
            dialog.Show();
            await closed.Task;
            return dialog.Result is TResult result ? result : default;
        }

        using var _ = (owner as IBlurrable)?.PushBlur();
        return await dialog.ShowDialog<TResult?>(owner);
    }

    private static Window? ResolveOwner()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.Windows.FirstOrDefault(w => w.IsActive) ?? lifetime?.MainWindow;
    }
}
