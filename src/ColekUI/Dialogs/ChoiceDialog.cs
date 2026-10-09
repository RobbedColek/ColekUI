using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ColekUI.Controls;
using ColekUI.Resources;

namespace ColekUI.Dialogs;

public sealed class ChoiceDialog : DialogWindow
{
    public ChoiceDialog(string title, string message, IReadOnlyList<string> options, string? cancelText = null)
    {
        Title = title;
        Header = title;
        BuiltInDialogSize.Apply(this);

        var optionsPanel = new StackPanel { Name = "OptionsPanel", Spacing = 8 };
        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            var button = new Button
            {
                Content = new TextBlock { Text = options[i], TextWrapping = TextWrapping.Wrap },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };
            button.Click += (_, _) => Close(index);
            optionsPanel.Children.Add(button);
        }

        Content = new StackPanel
        {
            Spacing = 8,
            Children = { new TextBlock { Text = message, Classes = { "dialog-message" } }, optionsPanel },
        };

        var cancel = new Button
        {
            Name = "CancelButton",
            MinWidth = 90,
            IsCancel = true,
            Content = cancelText ?? ColekStrings.Get("Cancel"),
        };

        cancel.Click += (_, _) => Close();
        Buttons.Add(cancel);
    }
}
