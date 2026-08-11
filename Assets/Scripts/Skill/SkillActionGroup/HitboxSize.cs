using UnityEngine;

namespace MagicSchool
{
    [System.Serializable]
    public class HitboxShape
    {
        public float size;
        [SerializeReference] public Shape shape;

        public HitboxShape(float size, Shape shape)
        {
            this.size = size;
            this.shape = shape;
        }
    }

    public interface Shape { }
    [System.Serializable] public class Box : Shape { }
    [System.Serializable] public class Circle : Shape { }
    [System.Serializable] public class Cone : Shape { }
}
