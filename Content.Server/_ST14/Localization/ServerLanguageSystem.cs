using System.Collections.Generic;
using System.Globalization;
using Content.Shared._ST14.Localization;
using Robust.Shared.Enums;
using Robust.Shared.Localization;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Server.Player;

namespace Content.Server._ST14.Localization;

public sealed partial class ServerLanguageSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private ILocalizationManager _localization = default!;

    private readonly Dictionary<NetUserId, CultureInfo> _cultures = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetLanguageMessage>(OnSetLanguage);
        _net.Disconnect += OnDisconnect;
        _player.PlayerStatusChanged += OnPlayerStatusChanged;

        Log.Info($"Server language: {_localization.DefaultCulture?.Name ?? Cultures.Default}");
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _net.Disconnect -= OnDisconnect;
        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    public CultureInfo? GetCulture(ICommonSession session)
    {
        return _cultures.GetValueOrDefault(session.UserId);
    }

    public CultureInfo GetEffectiveCulture(ICommonSession session)
    {
        return GetCulture(session) ?? _localization.DefaultCulture ?? new CultureInfo(Cultures.Default);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
            return;

        SendLanguage(args.Session);
    }

    private void OnSetLanguage(SetLanguageMessage message, EntitySessionEventArgs args)
    {
        if (Cultures.IsSupported(message.Culture))
            _cultures[args.SenderSession.UserId] = new CultureInfo(message.Culture);
        else
            _cultures.Remove(args.SenderSession.UserId);

        SendLanguage(args.SenderSession);
    }

    private void SendLanguage(ICommonSession session)
    {
        var effective = GetEffectiveCulture(session);

        Log.Debug($"Language for {session.Name}: {effective.Name}");

        RaiseNetworkEvent(new ServerLanguageMessage(effective.Name), session);
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs args)
    {
        _cultures.Remove(args.Channel.UserId);
    }
}
