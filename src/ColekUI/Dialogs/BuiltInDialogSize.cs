using Avalonia.Controls;
using Avalonia.Layout;
using ColekUI.Controls;

namespace ColekUI.Dialogs;

internal static class BuiltInDialogSize
{
    public const double MinWidth = 480;
    public const double MaxWidth = 640;
    public const double MessageMaxHeight = 360;

    public static void Apply(DialogWindow dialog)
    {
        dialog.SizeToContent = SizeToContent.WidthAndHeight;
        dialog.MinWidth = MinWidth;
        dialog.MaxWidth = MaxWidth;
        dialog.MinHeight = 0;
        dialog.ClearValue(Layoutable.WidthProperty);
        dialog.ClearValue(Layoutable.HeightProperty);
    }
}
