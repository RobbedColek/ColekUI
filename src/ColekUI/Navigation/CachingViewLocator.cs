using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ColekUI.Navigation;

public sealed class CachingViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _cache = new(ReferenceEqualityComparer.Instance);

    public Func<object, bool> Matches { get; init; } = data => data.GetType().Name.EndsWith("ViewModel", StringComparison.Ordinal);

    public Func<string, string> MapName { get; init; } = name => name.Replace("ViewModel", "View", StringComparison.Ordinal);

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        if (_cache.TryGetValue(data, out var cached))
            return cached;

        var name = MapName(data.GetType().FullName!);
        var type = data.GetType().Assembly.GetType(name);

        if (type is null)
            return new TextBlock { Text = "Not Found: " + name };

        var view = (Control)Activator.CreateInstance(type)!;
        _cache[data] = view;
        return view;
    }

    public bool Match(object? data) => data is not null && Matches(data);
}
