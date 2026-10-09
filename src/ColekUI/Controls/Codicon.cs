using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace ColekUI.Controls;

public sealed class Codicon : Control
{
    private const double GlyphSize = 16;

    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<Codicon, Geometry?>(nameof(Data));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<Codicon>();

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<Codicon, Stretch>(nameof(Stretch));

    static Codicon()
    {
        AffectsRender<Codicon>(DataProperty, ForegroundProperty, StretchProperty);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) => new(GlyphSize, GlyphSize);

    public override void Render(DrawingContext context)
    {
        if (Data is null || Foreground is null)
            return;

        using (context.PushTransform(CodiconLayout.Compute(Data.Bounds, Bounds.Size, Stretch)))
            context.DrawGeometry(Foreground, null, Data);
    }
}
