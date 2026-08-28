using UnityEngine;

namespace MagicSchool.Contracts
{
    // IEquipment answers: what can a hero wear?
    public interface IEquipment
    {
        string DisplayName { get; }
        Transform transform { get; }
    }
}
