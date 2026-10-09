using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColekUI.Dialogs;
using ColekUI.Navigation;
using ColekUI.Sample.Pages;
using ColekUI.Windowing;

namespace ColekUI.Sample;

public partial class App : Application
{
    internal static InMemoryPlacementStore PlacementStore { get; } = new();

    public static IDialogService Dialogs { get; } = new DialogService();

    public static bool NoRail { get; private set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            NoRail = desktop.Args?.Contains("--no-rail") == true;
            desktop.MainWindow = CreateWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindow CreateWindow()
    {
        var navigator = new Navigator(CreatePage);
        navigator.Items.Add(new NavigationItem("Dialogs", Icon("Icon.Checklist"), typeof(DialogsViewModel)));
        navigator.Items.Add(new NavigationItem("Windows", Icon("Icon.NewFile"), typeof(WindowsViewModel)));
        navigator.FooterItems.Add(new NavigationItem("Settings", Icon("Icon.SettingsGear"), typeof(SettingsViewModel)));
        navigator.NavigateTo<DialogsViewModel>();

        return new MainWindow
        {
            DataContext = navigator,
            PlacementKey = "Main",
            PlacementStore = PlacementStore,
            IsNavigationRailVisible = NoRail ? false : null,
        };
    }

    private static object CreatePage(Type pageType) => pageType.Name switch
    {
        nameof(DialogsViewModel) => new DialogsViewModel(),
        nameof(WindowsViewModel) => new WindowsViewModel(),
        nameof(SettingsViewModel) => new SettingsViewModel(),
        _ => throw new ArgumentException($"Unknown page {pageType}.", nameof(pageType)),
    };

    private static Geometry Icon(string key) => (Geometry)Current!.Resources[key]!;

    internal sealed class InMemoryPlacementStore : IWindowPlacementStore
    {
        private readonly Dictionary<string, WindowPlacement> _placements = [];

        public event Action? Changed;

        public WindowPlacement? Peek(string windowKey) => _placements.GetValueOrDefault(windowKey);

        public WindowPlacement? Load(string windowKey) => Peek(windowKey);

        public void Save(string windowKey, WindowPlacement placement)
        {
            _placements[windowKey] = placement;
            Changed?.Invoke();
        }
    }
}
