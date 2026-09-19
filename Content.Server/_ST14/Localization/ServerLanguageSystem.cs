using System.Collections.Generic;
using System.Globalization;
using Content.Shared._ST14.Localization;
using Robust.Shared.Localization;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._ST14.Localization;

public sealed partial class ServerLanguageSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private ILocalizationManager _localization = default!;

    private readonly Dictionary<NetUserId, CultureInfo> _cultures = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetLanguageMessage>(OnSetLanguage);
        _net.Disconnect += OnDisconnect;

        Log.Info($"Server language: {_localization.DefaultCulture?.Name ?? Cultures.Default}");
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _net.Disconnect -= OnDisconnect;
    }

    public CultureInfo? GetCulture(ICommonSession session)
    {
        return _cultures.GetValueOrDefault(session.UserId);
    }

    public CultureInfo GetEffectiveCulture(ICommonSession session)
    {
        return GetCulture(session) ?? _localization.DefaultCulture ?? new CultureInfo(Cultures.Default);
    }

    private void OnSetLanguage(SetLanguageMessage message, EntitySessionEventArgs args)
    {
        if (Cultures.IsSupported(message.Culture))
            _cultures[args.SenderSession.UserId] = new CultureInfo(message.Culture);
        else
            _cultures.Remove(args.SenderSession.UserId);

        var effective = GetEffectiveCulture(args.SenderSession);

        Log.Debug($"Language for {args.SenderSession.Name}: {effective.Name} (requested '{message.Culture}')");

        RaiseNetworkEvent(new ServerLanguageMessage(effective.Name), args.SenderSession);
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs args)
    {
        _cultures.Remove(args.Channel.UserId);
    }
}
