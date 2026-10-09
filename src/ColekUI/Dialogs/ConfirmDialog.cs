using Avalonia.Controls;
using ColekUI.Controls;
using ColekUI.Resources;

namespace ColekUI.Dialogs;

public sealed class ConfirmDialog : DialogWindow
{
    public ConfirmDialog(string title, string message, string? yesText = null, string? noText = null)
    {
        Title = title;
        Header = title;
        BuiltInDialogSize.Apply(this);

        Content = new TextBlock { Text = message, Classes = { "dialog-message" } };

        var yes = new Button { Name = "YesButton", MinWidth = 90, IsDefault = true, Content = yesText ?? ColekStrings.Get("Yes") };
        yes.Click += (_, _) => Close(true);
        var no = new Button { Name = "NoButton", MinWidth = 90, IsCancel = true, Content = noText ?? ColekStrings.Get("No") };
        no.Click += (_, _) => Close(false);
        Buttons.Add(yes);
        Buttons.Add(no);
    }
}
