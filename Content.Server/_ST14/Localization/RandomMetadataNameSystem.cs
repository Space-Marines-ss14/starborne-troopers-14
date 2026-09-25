using Content.Shared._ST14.Localization;
using Robust.Shared.GameStates;

namespace Content.Server._ST14.Localization;

public sealed partial class RandomMetadataNameSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomMetadataNameComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, RandomMetadataNameComponent component, ref ComponentGetState args)
    {
        args.State = new RandomMetadataNameComponentState(component.Format, component.Parts);
    }
}
