using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ColekUI.Navigation;

public interface INavigator : INotifyPropertyChanged
{
    ObservableCollection<NavigationItem> Items { get; }
    ObservableCollection<NavigationItem> FooterItems { get; }
    NavigationItem? SelectedItem { get; set; }
    object? CurrentPage { get; }
    void NavigateTo(Type pageType);
    void NavigateTo<TPage>();
}
