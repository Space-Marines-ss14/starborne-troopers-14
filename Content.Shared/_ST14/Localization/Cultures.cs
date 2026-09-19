using System.Globalization;
using System.Linq;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._ST14.Localization;

public static class Cultures
{
    public const string Default = "en-US";

    // Empty means "no explicit pick", the client follows the server language.
    public const string Auto = "";

    public static readonly string[] Supported =
    {
        "en-US",
        "ru-RU",
    };

    public static bool IsSupported(string culture)
    {
        return Supported.Contains(culture);
    }

    public static CultureInfo Resolve(IConfigurationManager configuration)
    {
        var chosen = configuration.GetCVar(ST14CVars.ClientLanguage);

        if (IsSupported(chosen))
            return new CultureInfo(chosen);

        return ResolveFallback(configuration);
    }

    public static CultureInfo ResolveServer(IConfigurationManager configuration)
    {
        var chosen = configuration.GetCVar(ST14CVars.ServerLanguage);

        if (IsSupported(chosen))
            return new CultureInfo(chosen);

        return ResolveFallback(configuration);
    }

    private static CultureInfo ResolveFallback(IConfigurationManager configuration)
    {
        var saved = configuration.GetCVar(CVars.LocCultureName);

        return new CultureInfo(IsSupported(saved) ? saved : Default);
    }
}
