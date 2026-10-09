using Avalonia.Interactivity;
using ColekUI.Controls;

namespace ColekUI.Sample.Pages;

public partial class RenameDialog : DialogWindow
{
    public RenameDialog() => InitializeComponent();

    private void OnRename(object? sender, RoutedEventArgs e) => Close(NameBox.Text);

    private async void OnCancel(object? sender, RoutedEventArgs e)
    {
        if (await App.Dialogs.ConfirmAsync("Discard", "Discard the new name?", owner: this))
            Close(null);
    }
}
