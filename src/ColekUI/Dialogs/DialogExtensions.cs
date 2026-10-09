using Avalonia.Controls;
using ColekUI.Controls;

namespace ColekUI.Dialogs;

public static class DialogExtensions
{
    public static Task<TResult?> ShowDialogAsync<TResult>(this Window owner, DialogWindow dialog) =>
        DialogService.ShowCoreAsync<TResult>(dialog, owner);

    public static Task ShowDialogAsync(this Window owner, DialogWindow dialog) =>
        DialogService.ShowCoreAsync<object>(dialog, owner);
}
