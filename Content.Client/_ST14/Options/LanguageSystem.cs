using System.Globalization;
using Content.Client.Gameplay;
using Content.Client._ST14.Localization;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Shared._ST14.Localization;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Localization;
using Robust.Shared.Player;

namespace Content.Client._ST14.Options;

public sealed partial class LanguageSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _configuration = default!;
    [Dependency] private ILocalizationManager _localization = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IStateManager _state = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private RandomMetadataNameSystem _randomNames = default!;

    private bool _applyQueued;
    private bool _rebuildQueued;

    public override void Initialize()
    {
        base.Initialize();

        _configuration.OnValueChanged(ST14CVars.ClientLanguage, OnLanguageChanged);
        _player.LocalSessionChanged += OnLocalSessionChanged;

        SubscribeNetworkEvent<ServerLanguageMessage>(OnServerLanguage);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.LocalSessionChanged -= OnLocalSessionChanged;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_applyQueued)
        {
            _applyQueued = false;
            ApplyCulture();
        }

        if (!_rebuildQueued)
            return;

        _rebuildQueued = false;

        SendLanguage();
        RebuildUi();
    }

    private void OnLanguageChanged(string language)
    {
        _applyQueued = true;
        _rebuildQueued = true;
    }

    private void OnLocalSessionChanged((ICommonSession? Old, ICommonSession? New) args)
    {
        SendLanguage();
    }

    private void OnServerLanguage(ServerLanguageMessage message, EntitySessionEventArgs args)
    {
        if (HasExplicitLanguage())
            return;

        if (!Cultures.IsSupported(message.Culture))
            return;

        var culture = new CultureInfo(message.Culture);

        if (_localization.DefaultCulture?.Name == culture.Name)
            return;

        ApplyCulture(culture);
        RebuildUi();
    }

    private bool HasExplicitLanguage()
    {
        return Cultures.IsSupported(_configuration.GetCVar(ST14CVars.ClientLanguage));
    }

    private void ApplyCulture()
    {
        if (!HasExplicitLanguage())
            return;

        ApplyCulture(new CultureInfo(_configuration.GetCVar(ST14CVars.ClientLanguage)));
    }

    private void ApplyCulture(CultureInfo culture)
    {
        if (_localization.DefaultCulture?.Name == culture.Name)
            return;

        _localization.SetCulture(culture);
        _localization.ReloadLocalizations();
    }

    private void SendLanguage()
    {
        if (_player.LocalSession == null)
            return;

        RaiseNetworkEvent(new SetLanguageMessage(_configuration.GetCVar(ST14CVars.ClientLanguage)));
    }

    private void RebuildUi()
    {
        if (_state.CurrentState is not GameplayState gameplay)
            return;

        var escape = _ui.GetUIController<EscapeUIController>();
        escape.OnStateExited(gameplay);
        escape.OnStateEntered(gameplay);

        _ui.GetUIController<OptionsUIController>().ReloadWindow();

        _randomNames.RefreshAll();

        gameplay.ReloadMainScreen();
    }
}
