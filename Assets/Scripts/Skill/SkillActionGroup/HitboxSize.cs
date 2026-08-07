using UnityEngine;

namespace MagicSchool
{
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
}
