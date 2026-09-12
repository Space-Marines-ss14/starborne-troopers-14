namespace Content.Client.Projectiles;

public sealed class ClientBulletTracerSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ClientBulletTracerComponent>();
        while (query.MoveNext(out var uid, out var tracer))
        {
            tracer.RemainingLifetime -= frameTime;

            if (tracer.RemainingLifetime <= 0f || Deleted(tracer.Anchor))
            {
                Del(uid);
                continue;
            }

            tracer.TraveledDistance += tracer.Speed * frameTime;

            // Anchor to the shooter/gun's CURRENT position every frame, not the position
            // at the moment of firing — this keeps the tracer visually "attached" to the
            // muzzle even if the player moves during its short lifetime.
            var anchorPos = _transform.GetWorldPosition(tracer.Anchor);
            var newPos = anchorPos + tracer.Direction * tracer.TraveledDistance;
            _transform.SetWorldPosition(uid, newPos);
        }
    }
}
