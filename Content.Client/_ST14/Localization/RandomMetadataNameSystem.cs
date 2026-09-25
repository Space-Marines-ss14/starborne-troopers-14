using Content.Shared._ST14.Localization;
using Robust.Shared.GameStates;

namespace Content.Client._ST14.Localization;

public sealed partial class RandomMetadataNameSystem : EntitySystem
{
    [Dependency] private MetaDataSystem _metaData = default!;

    private readonly HashSet<EntityUid> _pending = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomMetadataNameComponent, ComponentHandleState>(OnComponentHandleState);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pending.Count == 0)
            return;

        foreach (var uid in _pending)
        {
            Apply(uid);
        }

        _pending.Clear();
    }

    public void RefreshAll()
    {
        var query = EntityQueryEnumerator<RandomMetadataNameComponent>();

        while (query.MoveNext(out var uid, out _))
        {
            _pending.Add(uid);
        }
    }

    private void OnComponentHandleState(EntityUid uid, RandomMetadataNameComponent component, ref ComponentHandleState args)
    {
        Apply(uid, component);
        _pending.Add(uid);
    }

    private void Apply(EntityUid uid)
    {
        if (TryComp<RandomMetadataNameComponent>(uid, out var component))
            Apply(uid, component);
    }

    private void Apply(EntityUid uid, RandomMetadataNameComponent component)
    {
        if (component.Format.Length == 0 || component.Parts.Count == 0)
            return;

        var args = new (string, object)[component.Parts.Count];

        for (var i = 0; i < component.Parts.Count; i++)
        {
            args[i] = ($"part{i}", Loc.GetString(component.Parts[i]));
        }

        _metaData.SetEntityName(uid, Loc.GetString(component.Format, args), raiseEvents: false);
    }
}
