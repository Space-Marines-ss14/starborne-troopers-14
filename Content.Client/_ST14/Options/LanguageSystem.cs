using System;
using System.Globalization;
using Content.Client._ST14.Localization;
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
    private bool _restartPromptQueued;

    private LanguageRestartWindow? _restartWindow;

    private CultureInfo? _pendingCulture;

    private bool _synced;
    private int _syncAttempts;
    private float _syncTimer;

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

    // FrameUpdate since tick updates are skipped on non-predicted ticks
    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_applyQueued || _pendingCulture != null)
        {
            _applyQueued = false;

            var culture = _pendingCulture;
            _pendingCulture = null;

            if (culture != null)
                ApplyCulture(culture);
            else
                ApplyCulture();
        }

        SyncLanguage(frameTime);

        if (!_rebuildQueued)
            return;

        _rebuildQueued = false;

        RebuildUi();
    }

    // Resend until the server answers because the channel may not be ready yet
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

        // Screens are cached anyway so always offer a restart
        _restartPromptQueued = true;
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

    private void OnServerLanguage(ServerLanguageMessage message, EntitySessionEventArgs args)
    {
        _synced = true;

        if (!Cultures.IsSupported(message.Culture))
            return;

        var culture = new CultureInfo(message.Culture);

        // Client pick wins over the server language
        if (HasExplicitLanguage())
            return;

        if (_localization.DefaultCulture?.Name == culture.Name)
            return;

        // Queued because a network handler runs outside the tick
        _pendingCulture = culture;
        _rebuildQueued = true;
    }

    private bool HasExplicitLanguage()
    {
        return Cultures.IsSupported(_configuration.GetCVar(ST14CVars.ClientLanguage));
    }

    private void ApplyCulture()
    {
        // Same as server waits for the server reply to resolve it
        if (!HasExplicitLanguage())
            return;

        ApplyCulture(new CultureInfo(_configuration.GetCVar(ST14CVars.ClientLanguage)));
    }

    private bool ApplyCulture(CultureInfo culture)
    {
        if (_localization.DefaultCulture?.Name == culture.Name)
            return false;

        _localization.SetCulture(culture);
        _localization.ReloadLocalizations();
        return true;
    }

    private void SendLanguage()
    {
        if (_player.LocalSession == null)
            return;

        RaiseNetworkEvent(new SetLanguageMessage(_configuration.GetCVar(ST14CVars.ClientLanguage)));
    }

    // Screens are cached for the whole process so a restart prompt follows
    private void RebuildUi()
    {
        var previous = _state.CurrentState.GetType();

        // Focus points at a control we dispose and that crashes on refocus
        _ui.ReleaseKeyboardFocus();

        _state.RequestStateChange<LanguageReloadState>();

        // Stay in the empty state instead of leaving no UI at all
        if (_state.CurrentState is LanguageReloadState)
            _state.RequestStateChange(previous);

        _randomNames.RefreshAll();

        if (_restartPromptQueued)
        {
            _restartPromptQueued = false;
            ShowRestartPrompt();
        }
    }

    private void ShowRestartPrompt()
    {
        if (_restartWindow is { Disposed: false })
            _restartWindow.Close();

        _restartWindow = new LanguageRestartWindow();
        _restartWindow.OpenCentered();
    }
}
