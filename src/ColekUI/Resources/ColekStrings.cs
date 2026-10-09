using System.Globalization;
using System.Resources;

namespace ColekUI.Resources;

internal static class ColekStrings
{
    private static readonly ResourceManager Manager = new("ColekUI.Resources.Strings", typeof(ColekStrings).Assembly);

    public static string Get(string key) =>
        Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
