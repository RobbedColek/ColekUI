using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaControls = Avalonia.Controls.Controls;

namespace ColekUI.Controls;

[TemplatePart("PART_Frame", typeof(Border))]
[TemplatePart("PART_Header", typeof(TextBlock))]
[TemplatePart("PART_Description", typeof(TextBlock))]
[TemplatePart("PART_ButtonsPanel", typeof(ItemsControl))]
[TemplatePart("PART_BlurHost", typeof(Control))]
[TemplatePart("PART_ResizeGrips", typeof(Panel))]
[PseudoClasses(NoButtonsPseudoClass, TransparentPseudoClass)]
public class DialogWindow : Window, IBlurrable
{
    private const string NoButtonsPseudoClass = ":nobuttons";
    private const string TransparentPseudoClass = ":transparent";

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<DialogWindow, string?>(nameof(Header));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<DialogWindow, string?>(nameof(Description));

    public static readonly DirectProperty<DialogWindow, AvaloniaControls> ButtonsProperty =
        AvaloniaProperty.RegisterDirect<DialogWindow, AvaloniaControls>(nameof(Buttons), o => o.Buttons);

    public static readonly DirectProperty<DialogWindow, bool> IsBlurredProperty =
        AvaloniaProperty.RegisterDirect<DialogWindow, bool>(nameof(IsBlurred), o => o.IsBlurred);

    private readonly BlurState _blur = new();

    private Border? _frame;
    private Panel? _resizeGrips;
    private Control? _blurHost;

    public DialogWindow()
    {
        WindowDecorations = WindowDecorations.None;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        SizeToContent = SizeToContent.Height;

        _blur.Changed += OnBlurChanged;
        Buttons.CollectionChanged += (_, _) => UpdateButtonsState();
        UpdateButtonsState();
        UpdateTransparentState();
    }

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public AvaloniaControls Buttons { get; } = [];

    public bool IsBlurred
    {
        get;
        private set => SetAndRaise(IsBlurredProperty, ref field, value);
    }

    protected override Type StyleKeyOverride => typeof(DialogWindow);

    internal object? Result { get; private set; }

    public new void Close(object? dialogResult)
    {
        Result = dialogResult;
        base.Close(dialogResult);
    }

    public IDisposable PushBlur() => _blur.Push();

    protected override Size MeasureOverride(Size availableSize) =>
        WindowSizing.ClampToMinMax(this, base.MeasureOverride(availableSize));

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _frame?.PointerPressed -= OnFramePointerPressed;
        _resizeGrips?.PointerPressed -= OnResizeGripPointerPressed;

        _frame = e.NameScope.Find<Border>("PART_Frame");
        _resizeGrips = e.NameScope.Find<Panel>("PART_ResizeGrips");
        _blurHost = e.NameScope.Find<Control>("PART_BlurHost");

        _frame?.PointerPressed += OnFramePointerPressed;
        _resizeGrips?.PointerPressed += OnResizeGripPointerPressed;

        BlurState.ApplyEffect(_blurHost, IsBlurred);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Dispatcher.UIThread.Post(FocusFirstEditableControl, DispatcherPriority.Loaded);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!e.Handled && e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ActualTransparencyLevelProperty)
            UpdateTransparentState();
    }

    protected virtual void OnFramePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    protected virtual void OnResizeGripPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (CanResize && e.Source is Control { Tag: WindowEdge edge } &&
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(edge, e);
            e.Handled = true;
        }
    }

    private void OnBlurChanged()
    {
        IsBlurred = _blur.IsBlurred;
        BlurState.ApplyEffect(_blurHost, IsBlurred);
    }

    private void FocusFirstEditableControl()
    {
        if (FocusManager?.GetFocusedElement() is TextBox)
            return;

        this.GetVisualDescendants()
            .OfType<TextBox>()
            .FirstOrDefault(textBox => !textBox.IsReadOnly && textBox.Focusable &&
                                       textBox.IsEffectivelyEnabled && textBox.IsEffectivelyVisible)
            ?.Focus(NavigationMethod.Tab);
    }

    private void UpdateButtonsState() => PseudoClasses.Set(NoButtonsPseudoClass, Buttons.Count == 0);

    private void UpdateTransparentState() =>
        PseudoClasses.Set(TransparentPseudoClass, ActualTransparencyLevel != WindowTransparencyLevel.None);
}
