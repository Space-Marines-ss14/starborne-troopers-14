using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Shared.Projectiles;

public sealed class ProjectileDriftSystem : EntitySystem
{
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ProjectileComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var proj, out var physics))
        {
            if (proj.DriftRatePerSecond.Theta == 0)
                continue;

            var traveled = (_transform.GetWorldPosition(uid) - proj.SpawnPosition).Length();
            if (traveled < proj.DriftStartDistance)
                continue;

            var rotation = new Angle(proj.DriftRatePerSecond.Theta * frameTime * proj.DriftDirection);
            var newVelocity = rotation.RotateVec(physics.LinearVelocity);
            _physics.SetLinearVelocity(uid, newVelocity, body: physics);
        }
    }
}
