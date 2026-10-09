using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ColekUI.Controls;

internal sealed class NavigationIconPresenter : Decorator
{
    private const double IconSize = 20;

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<NavigationIconPresenter, object?>(nameof(Icon));

    private bool _attached;

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        Child = CreateChild(Icon);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
        Child = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty && _attached)
            Child = CreateChild(Icon);
    }

    private static Control? CreateChild(object? icon) => icon switch
    {
        null => null,
        Geometry geometry => new Codicon { Data = geometry, Stretch = Stretch.Uniform, Width = IconSize, Height = IconSize },
        Control control => control,
        _ => new ContentControl { Content = icon },
    };
}
