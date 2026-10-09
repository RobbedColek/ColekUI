using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ColekUI.Sample.Pages;

public partial class WindowsView : UserControl
{
    private Window? _window;

    public WindowsView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => Attach();
        DetachedFromVisualTree += (_, _) => Detach();
    }

    private void Attach()
    {
        App.PlacementStore.Changed += RefreshStored;
        RefreshStored();

        _window = TopLevel.GetTopLevel(this) as Window;
        if (_window is null)
            return;

        _window.PositionChanged += OnPositionChanged;
        _window.PropertyChanged += OnWindowPropertyChanged;
        RefreshCurrent();
    }

    private void Detach()
    {
        App.PlacementStore.Changed -= RefreshStored;

        if (_window is null)
            return;

        _window.PositionChanged -= OnPositionChanged;
        _window.PropertyChanged -= OnWindowPropertyChanged;
        _window = null;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e) => RefreshCurrent();

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.ClientSizeProperty || e.Property == Window.WindowStateProperty)
            RefreshCurrent();
    }

    private void RefreshStored() =>
        StoredText.Text = App.PlacementStore.Peek("Main") is { } p
            ? $"X {p.X}, Y {p.Y}, Width {p.Width:0}, Height {p.Height:0}, Maximized {p.IsMaximized}\nScreenLayout {p.ScreenLayout}"
            : "nothing stored yet";

    private void RefreshCurrent()
    {
        if (_window is null)
            return;

        CurrentText.Text = $"Position {_window.Position.X}, {_window.Position.Y}\n" +
                           $"ClientSize {_window.ClientSize.Width:0} x {_window.ClientSize.Height:0}\n" +
                           $"WindowState {_window.WindowState}";
    }

    private void OnOpenWindow(object? sender, RoutedEventArgs e) => App.CreateWindow().Show();
}
