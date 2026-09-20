using System;
using System.Globalization;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

namespace Content.Shared._ST14.Localization;

public readonly struct CultureScope : IDisposable
{
    private readonly ILocalizationManager _localization;
    private readonly CultureInfo? _previous;
    private readonly bool _changed;

    public CultureScope(CultureInfo culture)
    {
        _localization = IoCManager.Resolve<ILocalizationManager>();
        _previous = _localization.DefaultCulture;
        _changed = _previous == null || !string.Equals(_previous.Name, culture.Name, StringComparison.OrdinalIgnoreCase);

        if (_changed)
            _localization.SetCulture(culture);
    }

    public void Dispose()
    {
        if (_changed && _previous != null)
            _localization.SetCulture(_previous);
    }
}
