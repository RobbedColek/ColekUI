using Avalonia.Controls;
using ColekUI.Controls;

namespace ColekUI.Dialogs;

public interface IDialogService
{
    Task<TResult?> ShowAsync<TResult>(DialogWindow dialog, Window? owner = null);
    Task ShowAsync(DialogWindow dialog, Window? owner = null);
    Task<bool> ConfirmAsync(string title, string message, string? yesText = null, string? noText = null, Window? owner = null);
    Task ShowErrorAsync(string title, string message, string? okText = null, Window? owner = null);
    Task<int?> ChooseAsync(string title, string message, IReadOnlyList<string> options, string? cancelText = null, Window? owner = null);
}
