using UnityEngine;
using UnityEngine.Serialization;

namespace MagicSchool
{
    [System.Serializable]
    public class HitboxShape
    {
        [FormerlySerializedAs("size")] 
        public float Size;

        [FormerlySerializedAs("shape")]
        [SerializeReference] public Shape Shape;

        public HitboxShape(float size, Shape shape)
        {
            this.Size = size;
            this.Shape = shape;
        }
    }

    public interface Shape { }
    [System.Serializable] public class Box : Shape { }
    [System.Serializable] public class Circle : Shape { }
    [System.Serializable] public class Cone : Shape { }
}
