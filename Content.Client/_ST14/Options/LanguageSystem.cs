using System.Globalization;
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client._ST14.Localization;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Shared._ST14.Localization;
using Robust.Client;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Localization;
using Robust.Shared.Player;

namespace Content.Client._ST14.Options;

public sealed partial class LanguageSystem : EntitySystem
{
    private const int MaxSyncAttempts = 20;
    private const float SyncRetryDelay = 0.5f;

    [Dependency] private IBaseClient _client = default!;
    [Dependency] private IConfigurationManager _configuration = default!;
    [Dependency] private ILocalizationManager _localization = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IStateManager _state = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private RandomMetadataNameSystem _randomNames = default!;

    private bool _applyQueued;
    private bool _rebuildQueued;
    private bool _reloadQueued;

    private bool _synced;
    private int _syncAttempts;
    private float _syncTimer;

    // Applied on the next tick, never inside the button handler that set them.
    public void ForceReload()
    {
        _reloadQueued = true;
        _rebuildQueued = true;
    }

    public override void Initialize()
    {
        base.Initialize();

        _configuration.OnValueChanged(ST14CVars.ClientLanguage, OnLanguageChanged);
        _player.LocalSessionChanged += OnLocalSessionChanged;
        _client.RunLevelChanged += OnRunLevelChanged;

        SubscribeNetworkEvent<ServerLanguageMessage>(OnServerLanguage);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.LocalSessionChanged -= OnLocalSessionChanged;
        _client.RunLevelChanged -= OnRunLevelChanged;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_reloadQueued)
        {
            _reloadQueued = false;
            _localization.ReloadLocalizations();
        }

        if (_applyQueued)
        {
            _applyQueued = false;
            ApplyCulture();
        }

        SyncLanguage(frameTime);

        if (!_rebuildQueued)
            return;

        _rebuildQueued = false;

        RebuildUi();
    }

    // Resend until the server answers: the channel may not be ready yet on connect.
    private void SyncLanguage(float frameTime)
    {
        if (_player.LocalSession == null)
        {
            ResetSync();
            return;
        }

        if (_synced || _syncAttempts >= MaxSyncAttempts)
            return;

        _syncTimer -= frameTime;

        if (_syncTimer > 0f)
            return;

        _syncTimer = SyncRetryDelay;
        _syncAttempts++;

        SendLanguage();
    }

    private void ResetSync()
    {
        _synced = false;
        _syncAttempts = 0;
        _syncTimer = 0f;
    }

    private void OnLanguageChanged(string language)
    {
        _applyQueued = true;
        _rebuildQueued = true;
        ResetSync();
    }

    private void OnLocalSessionChanged((ICommonSession? Old, ICommonSession? New) args)
    {
        ResetSync();
        SendLanguage();
    }

    private void OnRunLevelChanged(object? sender, RunLevelChangedEventArgs args)
    {
        ResetSync();
        SendLanguage();
    }

    // An explicit client pick always beats the server language.
    private void OnServerLanguage(ServerLanguageMessage message, EntitySessionEventArgs args)
    {
        _synced = true;

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

    // XAML {Loc} is resolved once at load, so the windows have to be recreated.
    private void RebuildUi()
    {
        switch (_state.CurrentState)
        {
            case GameplayState gameplay:
                var escape = _ui.GetUIController<EscapeUIController>();
                escape.OnStateExited(gameplay);
                escape.OnStateEntered(gameplay);

                _ui.GetUIController<OptionsUIController>().ReloadWindow();

                _randomNames.RefreshAll();

                gameplay.ReloadMainScreen();
                break;

            case LobbyState lobby:
                var lobbyUi = _ui.GetUIController<LobbyUIController>();
                lobbyUi.OnStateExited(lobby);
                lobbyUi.OnStateEntered(lobby);
                break;
        }
    }
}
