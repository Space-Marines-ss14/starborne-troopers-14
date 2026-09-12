namespace Content.Shared.Damage;

/// <summary>
/// Shared distance-based damage falloff calculation, used by hitscan and projectile weapons.
/// </summary>
public static class DamageFalloffHelper
{
    /// <summary>
    /// Returns a damage multiplier between 1 (full damage) and minMultiplier,
    /// linearly interpolated between falloffStart and falloffEnd distances.
    /// </summary>
    public static float GetMultiplier(float distance, float falloffStart, float falloffEnd, float minMultiplier)
    {
        if (distance <= falloffStart)
            return 1f;

        if (distance >= falloffEnd)
            return minMultiplier;

        var t = (distance - falloffStart) / (falloffEnd - falloffStart);
        return MathHelper.Lerp(1f, minMultiplier, t);
    }
}
