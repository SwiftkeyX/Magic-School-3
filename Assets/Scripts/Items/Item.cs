using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Item : MonoBehaviour, IInspectableItem, IDraggable, IEquipment
    {
        [SerializeField] private ItemDataSO _data;
        public ItemDataSO Data => _data;
        private ICustomModifier _modifier;

        // ================================= getter =================================
        public ICustomModifier Modifier => _modifier;
        // === IInspectableItem ===
        public string DisplayName => _data != null ? _data.Name : name;
        public string Description => _data != null ? _data.Description : string.Empty;
        public bool IsAlive => this != null;    

        void Awake()
        {
            _modifier = ItemLibrary.Resolve(_data != null ? _data.ItemId : ItemIdEnum.None);
        }
    }
}
