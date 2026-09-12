using System.Numerics;

namespace Content.Client.Projectiles;

[RegisterComponent]
public sealed partial class ClientBulletTracerComponent : Component
{
    public EntityUid Anchor;          // сущность, за которой следует трассер (пушка/стрелок)
    public Vector2 Direction;         // фиксированное мировое направление выстрела
    public float Speed;
    public float TraveledDistance;
    public float RemainingLifetime;
}
