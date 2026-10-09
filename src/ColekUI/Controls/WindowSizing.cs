using Avalonia;
using Avalonia.Layout;

namespace ColekUI.Controls;

internal static class WindowSizing
{
    internal static Size ClampToMinMax(Layoutable window, Size desired) => new(
        Math.Max(Math.Min(desired.Width, window.MaxWidth), window.MinWidth),
        Math.Max(Math.Min(desired.Height, window.MaxHeight), window.MinHeight));
}