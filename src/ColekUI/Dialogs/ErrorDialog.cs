using Avalonia.Controls;
using ColekUI.Controls;
using ColekUI.Resources;

namespace ColekUI.Dialogs;

public sealed class ErrorDialog : DialogWindow
{
    public ErrorDialog(string title, string message, string? okText = null)
    {
        Title = title;
        Header = title;
        BuiltInDialogSize.Apply(this);

        Content = new ScrollViewer
        {
            MaxHeight = BuiltInDialogSize.MessageMaxHeight,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = new SelectableTextBlock { Name = "MessageText", Text = message, Classes = { "dialog-message" } },
        };

        var ok = new Button { Name = "OkButton", MinWidth = 90, IsDefault = true, IsCancel = true, Content = okText ?? ColekStrings.Get("Ok") };
        ok.Click += (_, _) => Close();
        Buttons.Add(ok);
    }
}
