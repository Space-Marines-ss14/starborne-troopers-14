using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;

namespace Content.Shared.Weapons.Hitscan.Systems;

public sealed partial class HitscanBasicDamageSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanBasicDamageComponent, HitscanRaycastFiredEvent>(OnHitscanHit);
    }

    private void OnHitscanHit(Entity<HitscanBasicDamageComponent> ent, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        var falloff = GetFalloffMultiplier(ent.Comp, args.Data.Distance);
        var dmg = ent.Comp.Damage * _damage.UniversalHitscanDamageModifier * falloff;

        if (!_damage.TryChangeDamage(args.Data.HitEntity.Value, dmg, out var damageDealt, origin: args.Data.Gun))
            return;

        var damageEvent = new HitscanDamageDealtEvent
        {
            Target = args.Data.HitEntity.Value,
            DamageDealt = damageDealt,
        };

        RaiseLocalEvent(ent, ref damageEvent);
    }

    private static float GetFalloffMultiplier(HitscanBasicDamageComponent comp, float distance)
    {
        if (distance <= comp.FalloffStart)
            return 1f;

        if (distance >= comp.FalloffEnd)
            return comp.MinDamageMultiplier;

        var t = (distance - comp.FalloffStart) / (comp.FalloffEnd - comp.FalloffStart);
        return MathHelper.Lerp(1f, comp.MinDamageMultiplier, t);
    }
}
