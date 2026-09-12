using System.Numerics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using TimedDespawnComponent = Robust.Shared.Spawners.TimedDespawnComponent;

namespace Content.Client.Projectiles;

public sealed partial class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private AnimationPlayerSystem _player = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedTransformSystem _transformSystem = default!;

    private readonly Dictionary<EntityUid, Vector2> _pendingReveal = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ImpactEffectEvent>(OnProjectileImpact);
        SubscribeLocalEvent<ProjectileComponent, ComponentStartup>(OnProjectileStartup);
        SubscribeLocalEvent<ProjectileComponent, ComponentShutdown>(OnProjectileShutdown);
    }

    private void OnProjectileStartup(EntityUid uid, ProjectileComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.Visible = false;
        _pendingReveal[uid] = _transformSystem.GetWorldPosition(uid);
    }

    private void OnProjectileShutdown(EntityUid uid, ProjectileComponent component, ComponentShutdown args)
    {
        _pendingReveal.Remove(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingReveal.Count == 0)
            return;

        // Reveal each projectile's sprite only once it has actually moved from its spawn
        // position — this hides the single static frame before network interpolation
        // catches up, instead of showing the bullet "hanging" in place.
        List<EntityUid>? toRemove = null;

        foreach (var (uid, lastPos) in _pendingReveal)
        {
            if (Deleted(uid) || !TryComp<SpriteComponent>(uid, out var sprite))
            {
                toRemove ??= new List<EntityUid>();
                toRemove.Add(uid);
                continue;
            }

            var currentPos = _transformSystem.GetWorldPosition(uid);

            if ((currentPos - lastPos).LengthSquared() > 0.0001f)
            {
                sprite.Visible = true;
                toRemove ??= new List<EntityUid>();
                toRemove.Add(uid);
            }
        }

        if (toRemove != null)
        {
            foreach (var uid in toRemove)
                _pendingReveal.Remove(uid);
        }
    }

    private void OnProjectileImpact(ImpactEffectEvent ev)
    {
        var coords = GetCoordinates(ev.Coordinates);

        if (Deleted(coords.EntityId))
            return;

        var ent = Spawn(ev.Prototype, coords);

        if (TryComp<SpriteComponent>(ent, out var sprite))
        {
            sprite[EffectLayers.Unshaded].AutoAnimated = false;
            _sprite.LayerMapTryGet((ent, sprite), EffectLayers.Unshaded, out var layer, false);
            var state = _sprite.LayerGetRsiState((ent, sprite), layer);
            var lifetime = 0.5f;

            if (TryComp<TimedDespawnComponent>(ent, out var despawn))
                lifetime = despawn.Lifetime;

            var anim = new Animation()
            {
                Length = TimeSpan.FromSeconds(lifetime),
                AnimationTracks =
                {
                    new AnimationTrackSpriteFlick()
                    {
                        LayerKey = EffectLayers.Unshaded,
                        KeyFrames =
                        {
                            new AnimationTrackSpriteFlick.KeyFrame(state.Name, 0f),
                        }
                    }
                }
            };

            _player.Play(ent, anim, "impact-effect");
        }
    }
}
