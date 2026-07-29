using UnityEngine;

/// <summary>
/// AOE here = Area of effect. AOE could have size = 1.
/// So AOE doesn't neccesary mean it hit multiple target.
/// </summary>
[System.Serializable]
public class HitboxSize
{
    public float size;
    [SerializeReference] public HitboxShape shape;

    public HitboxSize() : this(1f, null) { }

    public HitboxSize(float size, HitboxShape shape)
    {
        this.size = size;
        this.shape = shape;
    }
}

public interface HitboxShape {}
[System.Serializable] public class Box: HitboxShape {}
[System.Serializable] public class Circle: HitboxShape {}
[System.Serializable] public class Cone: HitboxShape {}
