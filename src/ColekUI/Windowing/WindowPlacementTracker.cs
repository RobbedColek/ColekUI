using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace ColekUI.Windowing;

internal readonly record struct ScreenInfo(PixelRect Bounds, PixelRect WorkingArea, double Scaling, bool IsPrimary);

public sealed class WindowPlacementTracker
{
    private readonly Window _window;
    private readonly IWindowPlacementStore _store;
    private readonly string _windowKey;

    private PixelPoint _startPosition;
    private Size _startSize;

    private WindowPlacementTracker(Window window, IWindowPlacementStore store, string windowKey)
    {
        _window = window;
        _store = store;
        _windowKey = windowKey;
    }

    public static void Attach(
        Window window, IWindowPlacementStore store, string windowKey, double defaultSizeRatio = 0.9, Size? defaultSize = null)
    {
        WindowPlacementTracker tracker = new(window, store, windowKey);
        tracker.ApplyInitialPlacement(defaultSizeRatio, defaultSize);
        tracker.StartTracking();
    }

    internal static string DescribeScreens(IEnumerable<ScreenInfo> screens) =>
        string.Join(';', screens
            .OrderBy(s => s.Bounds.X)
            .ThenBy(s => s.Bounds.Y)
            .Select(s => string.Create(CultureInfo.InvariantCulture,
                $"{s.Bounds.X},{s.Bounds.Y},{s.Bounds.Width},{s.Bounds.Height}@{s.Scaling:R}{(s.IsPrimary ? "*" : "")}")));

    internal static (PixelPoint Position, Size Size, bool Maximize)? ComputeInitial(
        WindowPlacement? saved, IReadOnlyList<ScreenInfo> screens, double ratio, Size? defaultSize = null)
    {
        if (screens.Count == 0 || !screens.Any(s => s.IsPrimary))
            return null;

        if (saved is not null && saved.ScreenLayout == DescribeScreens(screens))
            return (new PixelPoint(saved.X, saved.Y), new Size(saved.Width, saved.Height), saved.IsMaximized);

        ScreenInfo primary = screens.First(s => s.IsPrimary);
        PixelRect workingArea = primary.WorkingArea;

        double areaWidth = workingArea.Width / primary.Scaling;
        double areaHeight = workingArea.Height / primary.Scaling;
        (double width, double height) = defaultSize is { } size
            ? (Math.Min(size.Width, areaWidth), Math.Min(size.Height, areaHeight))
            : (areaWidth * ratio, areaHeight * ratio);

        return (
            new PixelPoint(
                workingArea.X + (int)((workingArea.Width - width * primary.Scaling) / 2),
                workingArea.Y + (int)((workingArea.Height - height * primary.Scaling) / 2)),
            new Size(width, height),
            false);
    }

    private IReadOnlyList<ScreenInfo> ReadScreens() =>
        _window.Screens.All.Select(s => new ScreenInfo(s.Bounds, s.WorkingArea, s.Scaling, s.IsPrimary)).ToList();

    private void ApplyInitialPlacement(double defaultSizeRatio, Size? defaultSize)
    {
        IReadOnlyList<ScreenInfo> screens = ReadScreens();
        if (screens.Count == 0 || !screens.Any(s => s.IsPrimary))
            return;

        WindowPlacement? saved = null;
        try
        {
            saved = _store.Load(_windowKey);
        }
        catch (Exception)
        {
            //
        }

        if (ComputeInitial(saved, screens, defaultSizeRatio, defaultSize) is not { } initial)
            return;

        ApplyBounds(initial.Position, initial.Size);

        if (initial.Maximize)
            _window.WindowState = WindowState.Maximized;
    }

    private void ApplyBounds(PixelPoint position, Size size)
    {
        _window.WindowStartupLocation = WindowStartupLocation.Manual;
        _window.Width = size.Width;
        _window.Height = size.Height;
        _window.Position = position;

        _startPosition = position;
        _startSize = size;
    }

    private void StartTracking()
    {
        _window.Closing += (_, e) =>
        {
            if (!e.Cancel)
                SavePlacement();
        };
    }

    private void SavePlacement()
    {
        bool isNormal = _window.WindowState == WindowState.Normal;
        PixelPoint position = isNormal ? _window.Position : _startPosition;
        Size size = isNormal ? _window.ClientSize : _startSize;

        try
        {
            _store.Save(_windowKey, new WindowPlacement(
                DescribeScreens(ReadScreens()),
                position.X,
                position.Y,
                size.Width,
                size.Height,
                _window.WindowState == WindowState.Maximized));
        }
        catch (Exception)
        {
            //
        }
    }
}
