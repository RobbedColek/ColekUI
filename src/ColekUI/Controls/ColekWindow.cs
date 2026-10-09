using Avalonia.Automation;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ColekUI.Navigation;
using ColekUI.Resources;
using ColekUI.Windowing;

namespace ColekUI.Controls;

[TemplatePart("PART_TitleBar", typeof(Control))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeIcon", typeof(Codicon))]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_Rail", typeof(Control))]
[TemplatePart("PART_NavigationList", typeof(ListBox))]
[TemplatePart("PART_FooterList", typeof(ListBox))]
[TemplatePart("PART_ContentPanel", typeof(Control))]
[TemplatePart("PART_BlurHost", typeof(Control))]
[PseudoClasses(NoRailPseudoClass, MaximizedPseudoClass)]
public class ColekWindow : Window, IBlurrable
{
    private const string NoRailPseudoClass = ":norail";
    private const string MaximizedPseudoClass = ":maximized";

    public static readonly StyledProperty<IEnumerable<NavigationItem>?> NavigationItemsProperty =
        AvaloniaProperty.Register<ColekWindow, IEnumerable<NavigationItem>?>(nameof(NavigationItems));

    public static readonly StyledProperty<IEnumerable<NavigationItem>?> FooterNavigationItemsProperty =
        AvaloniaProperty.Register<ColekWindow, IEnumerable<NavigationItem>?>(nameof(FooterNavigationItems));

    public static readonly StyledProperty<NavigationItem?> SelectedNavigationItemProperty =
        AvaloniaProperty.Register<ColekWindow, NavigationItem?>(
            nameof(SelectedNavigationItem), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool?> IsNavigationRailVisibleProperty =
        AvaloniaProperty.Register<ColekWindow, bool?>(nameof(IsNavigationRailVisible));

    public static readonly DirectProperty<ColekWindow, bool> IsBlurredProperty =
        AvaloniaProperty.RegisterDirect<ColekWindow, bool>(nameof(IsBlurred), o => o.IsBlurred);

    public static readonly StyledProperty<string?> PlacementKeyProperty =
        AvaloniaProperty.Register<ColekWindow, string?>(nameof(PlacementKey));

    public static readonly StyledProperty<IWindowPlacementStore?> PlacementStoreProperty =
        AvaloniaProperty.Register<ColekWindow, IWindowPlacementStore?>(nameof(PlacementStore));

    public static readonly StyledProperty<double> DefaultSizeRatioProperty =
        AvaloniaProperty.Register<ColekWindow, double>(nameof(DefaultSizeRatio), 0.9);

    private readonly BlurState _blur = new();

    private Control? _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;
    private ListBox? _navigationList;
    private ListBox? _footerList;
    private Control? _blurHost;

    private bool _syncingSelection;
    private bool _selectionSyncScheduled;
    private bool _placementAttached;

    public ColekWindow()
    {
        _blur.Changed += OnBlurChanged;
        AddHandler(WindowOpenedEvent, (_, _) => TryAttachPlacement());
        UpdateRailVisibility();
        UpdateMaximizedState();
    }

    public IEnumerable<NavigationItem>? NavigationItems
    {
        get => GetValue(NavigationItemsProperty);
        set => SetValue(NavigationItemsProperty, value);
    }

    public IEnumerable<NavigationItem>? FooterNavigationItems
    {
        get => GetValue(FooterNavigationItemsProperty);
        set => SetValue(FooterNavigationItemsProperty, value);
    }

    public NavigationItem? SelectedNavigationItem
    {
        get => GetValue(SelectedNavigationItemProperty);
        set => SetValue(SelectedNavigationItemProperty, value);
    }

    public bool? IsNavigationRailVisible
    {
        get => GetValue(IsNavigationRailVisibleProperty);
        set => SetValue(IsNavigationRailVisibleProperty, value);
    }

    public bool IsBlurred
    {
        get;
        private set => SetAndRaise(IsBlurredProperty, ref field, value);
    }

    public string? PlacementKey
    {
        get => GetValue(PlacementKeyProperty);
        set => SetValue(PlacementKeyProperty, value);
    }

    public IWindowPlacementStore? PlacementStore
    {
        get => GetValue(PlacementStoreProperty);
        set => SetValue(PlacementStoreProperty, value);
    }

    public double DefaultSizeRatio
    {
        get => GetValue(DefaultSizeRatioProperty);
        set => SetValue(DefaultSizeRatioProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ColekWindow);

    public IDisposable PushBlur() => _blur.Push();

    protected override Size MeasureOverride(Size availableSize) =>
        WindowSizing.ClampToMinMax(this, base.MeasureOverride(availableSize));

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _titleBar?.PointerPressed -= OnTitleBarPointerPressed;
        _titleBar?.DoubleTapped -= OnTitleBarDoubleTapped;
        _minimizeButton?.Click -= OnMinimizeButtonClick;
        _maximizeButton?.Click -= OnMaximizeButtonClick;
        _closeButton?.Click -= OnCloseButtonClick;
        RemoveToolTipHandler(_minimizeButton);
        RemoveToolTipHandler(_maximizeButton);
        RemoveToolTipHandler(_closeButton);
        _navigationList?.SelectionChanged -= OnListSelectionChanged;
        _footerList?.SelectionChanged -= OnListSelectionChanged;

        _titleBar = e.NameScope.Find<Control>("PART_TitleBar");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
        _maximizeButton = e.NameScope.Find<Button>("PART_MaximizeButton");
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _navigationList = e.NameScope.Find<ListBox>("PART_NavigationList");
        _footerList = e.NameScope.Find<ListBox>("PART_FooterList");
        _blurHost = e.NameScope.Find<Control>("PART_BlurHost");

        _titleBar?.PointerPressed += OnTitleBarPointerPressed;
        _titleBar?.DoubleTapped += OnTitleBarDoubleTapped;
        _minimizeButton?.Click += OnMinimizeButtonClick;
        _maximizeButton?.Click += OnMaximizeButtonClick;
        _closeButton?.Click += OnCloseButtonClick;
        _navigationList?.SelectionChanged += OnListSelectionChanged;
        _footerList?.SelectionChanged += OnListSelectionChanged;

        AddToolTipHandler(_minimizeButton);
        AddToolTipHandler(_maximizeButton);
        AddToolTipHandler(_closeButton);

        UpdateMaximizedState();
        UpdateBlurEffect();
        SyncListSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NavigationItemsProperty || change.Property == FooterNavigationItemsProperty)
        {
            if (change.OldValue is INotifyCollectionChanged oldItems)
                oldItems.CollectionChanged -= OnNavigationItemsCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newItems)
                newItems.CollectionChanged += OnNavigationItemsCollectionChanged;

            UpdateRailVisibility();
            ScheduleSelectionSync();
        }
        else if (change.Property == IsNavigationRailVisibleProperty)
        {
            UpdateRailVisibility();
        }
        else if (change.Property == SelectedNavigationItemProperty)
        {
            SyncListSelection();
        }
        else if (change.Property == WindowStateProperty)
        {
            UpdateMaximizedState();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (NavigationItems is INotifyCollectionChanged items)
            items.CollectionChanged -= OnNavigationItemsCollectionChanged;
        if (FooterNavigationItems is INotifyCollectionChanged footerItems)
            footerItems.CollectionChanged -= OnNavigationItemsCollectionChanged;
    }

    protected virtual void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    protected virtual void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e) => ToggleMaximized();

    protected virtual void OnMinimizeButtonClick(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    protected virtual void OnMaximizeButtonClick(object? sender, RoutedEventArgs e) => ToggleMaximized();

    protected virtual void OnCloseButtonClick(object? sender, RoutedEventArgs e) => Close();

    protected virtual void ToggleMaximized() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection || sender is not ListBox { SelectedItem: NavigationItem item } list)
            return;

        SetCurrentValue(SelectedNavigationItemProperty, item);

        if (!Equals(SelectedNavigationItem, item))
        {
            SyncListSelection();
            return;
        }

        _syncingSelection = true;
        try
        {
            (list == _navigationList ? _footerList : _navigationList)?.SelectedItem = null;
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void SyncListSelection()
    {
        NavigationItem? item = SelectedNavigationItem;
        bool inTop = item is not null && Contains(NavigationItems, item);
        bool inFooter = item is not null && !inTop && Contains(FooterNavigationItems, item);

        _syncingSelection = true;
        try
        {
            _navigationList?.SelectedItem = inTop ? item : null;
            _footerList?.SelectedItem = inFooter ? item : null;
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void OnNavigationItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateRailVisibility();
        ScheduleSelectionSync();
    }

    private void ScheduleSelectionSync()
    {
        if (_selectionSyncScheduled)
            return;

        _selectionSyncScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            _selectionSyncScheduled = false;
            SyncListSelection();
        });
    }

    private void UpdateRailVisibility()
    {
        bool visible = IsNavigationRailVisible ?? (HasItems(NavigationItems) || HasItems(FooterNavigationItems));
        PseudoClasses.Set(NoRailPseudoClass, !visible);
    }

    private void UpdateMaximizedState()
    {
        bool maximized = WindowState == WindowState.Maximized;
        PseudoClasses.Set(MaximizedPseudoClass, maximized);
    }

    private void AddToolTipHandler(Button? button)
    {
        if (button is null)
            return;

        ApplyWindowButtonText(button);
        ToolTip.AddToolTipOpeningHandler(button, OnWindowButtonToolTipOpening);
    }

    private void RemoveToolTipHandler(Button? button)
    {
        if (button is not null)
            ToolTip.RemoveToolTipOpeningHandler(button, OnWindowButtonToolTipOpening);
    }

    private void OnWindowButtonToolTipOpening(object? sender, CancelRoutedEventArgs e)
    {
        if (sender is Button button)
            ApplyWindowButtonText(button);
    }

    private void ApplyWindowButtonText(Button button)
    {
        string key = button == _minimizeButton ? "Minimize"
            : button == _closeButton ? "Close"
            : WindowState == WindowState.Maximized ? "Restore"
            : "Maximize";
        string text = ColekStrings.Get(key);
        ToolTip.SetTip(button, text);
        AutomationProperties.SetName(button, text);
    }

    private void OnBlurChanged()
    {
        IsBlurred = _blur.IsBlurred;
        UpdateBlurEffect();
    }

    private void UpdateBlurEffect() => BlurState.ApplyEffect(_blurHost, IsBlurred);

    private void TryAttachPlacement()
    {
        if (_placementAttached || PlacementKey is not { } key || PlacementStore is not { } store) return;

        _placementAttached = true;
        WindowPlacementTracker.Attach(this, store, key, DefaultSizeRatio);
    }

    private static bool HasItems(IEnumerable<NavigationItem>? items) => items?.Any() == true;

    private static bool Contains(IEnumerable<NavigationItem>? items, NavigationItem item) =>
        items?.Contains(item) == true;
}
