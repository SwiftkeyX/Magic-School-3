using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Item : MonoBehaviour, IInspectableItem, IDraggable
    {
        [SerializeField] private ItemDataSO _data;
        public ItemDataSO Data => _data;

        // === IInspectableItem ===
        public string DisplayName => _data != null ? _data.Name : name;
        public string Description => _data != null ? _data.Description : string.Empty;
        public bool IsAlive => this != null;    
    }
}
