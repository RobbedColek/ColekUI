using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using Avalonia.Threading;

namespace ColekUI.Navigation;

public sealed class Navigator(Func<Type, object> pageFactory) : INavigator
{
    private NavigationItem? _selectedItem;
    private object? _currentPage;
    private int _version;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<NavigationItem> Items { get; } = [];

    public ObservableCollection<NavigationItem> FooterItems { get; } = [];

    public NavigationItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Equals(_selectedItem, value))
                return;

            int version = ++_version;
            object? page;
            try
            {
                page = value is null ? null : pageFactory(value.PageType);
                (page as INavigationAware)?.OnNavigatedTo();
            }
            catch (Exception ex)
            {
                var error = ExceptionDispatchInfo.Capture(ex);
                Dispatcher.UIThread.Post(error.Throw);
                if (version == _version)
                    ResyncSelectedItem();
                return;
            }

            if (version != _version)
                return;

            _selectedItem = value;
            OnPropertyChanged(nameof(SelectedItem));

            if (page is null)
                return;

            _currentPage = page;
            OnPropertyChanged(nameof(CurrentPage));
        }
    }

    public object? CurrentPage => _currentPage;

    public void NavigateTo(Type pageType)
    {
        var item = Items.Concat(FooterItems).FirstOrDefault(i => i.PageType == pageType)
            ?? throw new InvalidOperationException($"No navigation item for page type {pageType}.");
        SelectedItem = item;
    }

    public void NavigateTo<TPage>() => NavigateTo(typeof(TPage));

    private void ResyncSelectedItem()
    {
        var previous = _selectedItem;
        _selectedItem = null;
        OnPropertyChanged(nameof(SelectedItem));
        _selectedItem = previous;
        OnPropertyChanged(nameof(SelectedItem));
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
