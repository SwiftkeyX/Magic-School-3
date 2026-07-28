public enum ActionSource
{
    Self, 
    Ally,
    Summon,

    // ...
}

public enum ActionName
{
    // ====================== AOE ======================
    ZoneAOE,
    CircleAOE,

    // ====================== Laser ======================
    LaserShot,

    // ======================== Etc ======================
    Cast,

    // ...
}

public enum AimTarget
{
    Self,
    Current,
    Furthest,
    Clustered,
    
    // ...
}