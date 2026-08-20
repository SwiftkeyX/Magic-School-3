namespace MagicSchool.Skills
{
    /// <summary>
    /// Which template action a skill wants to play.
    /// 
    /// This is enum used to reference matched prefab. 
    /// E.g. projectile prefab
    /// </summary>
    public enum TemplateActionEnum
    {
        None = 0,

        // ==================================== AOE ====================================
        BoxAOETip = 1,
        CircleAOE = 2,
        CircleAOESticky = 15,
        HalfCircleAOESticky = 17,
        CircleAOETip = 3,
        CircleAOEJarvanIvVariant = 14,
        TriangleAOETip = 4,
        ZoneAOE = 5,
        ZoneAOEGarenVariant = 6,

        // ==================================== Projectile ====================================
        FirstHitProjectile = 7,
        HomingProjectile = 8,
        PiercingProjectile = 9,
        PiercingProjectileSonaVariant = 12,

        // ==================================== Other ====================================
        Cast = 10,
        CastGalioVariant = 11,
        Move = 13,
        MoveSionVariant = 16,
    }
}
