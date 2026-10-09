using Avalonia.Controls;
using Avalonia.Interactivity;
using ColekUI.Controls;
using ColekUI.Dialogs;

namespace ColekUI.Sample.Pages;

public partial class DialogsView : UserControl
{
    public DialogsView() => InitializeComponent();

    private async void OnConfirm(object? sender, RoutedEventArgs e)
    {
        var yes = await App.Dialogs.ConfirmAsync("Are you sure?", "Are you really sure you want to do this?");
        ResultText.Text = $"Confirm: {yes}";
    }

    private async void OnError(object? sender, RoutedEventArgs e)
    {
        await App.Dialogs.ShowErrorAsync("Something failed", "The operation could not be completed.");
        ResultText.Text = "Error dialog closed";
    }

    private async void OnChoose3(object? sender, RoutedEventArgs e)
    {
        var index = await App.Dialogs.ChooseAsync("Choice", "Pick something.", ["Potato", "Sandwich", "Doughnut"]);
        ResultText.Text = $"Choice: {index?.ToString() ?? "cancelled"}";
    }

    private async void OnChoose0(object? sender, RoutedEventArgs e)
    {
        var index = await App.Dialogs.ChooseAsync("Nothing to choose", "There are no options, so only Cancel is offered.", []);
        ResultText.Text = $"Choice: {index?.ToString() ?? "cancelled"}";
    }

    private async void OnCustom(object? sender, RoutedEventArgs e)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        var name = await owner!.ShowDialogAsync<string>(new RenameDialog());
        ResultText.Text = $"Custom dialog: {name ?? "cancelled"}";
    }

    private async void OnPickFile(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
            return;

        using (window is IBlurrable blurrable ? blurrable.PushBlur() : null)
        {
            var files = await window.StorageProvider.OpenFilePickerAsync(new() { Title = "Pick a file" });
            ResultText.Text = $"File: {files.FirstOrDefault()?.Name ?? "none"}";
        }
    }
}
