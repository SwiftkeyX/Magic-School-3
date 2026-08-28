using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes
{
    internal class HeroItems
    {
        public const int Capacity = 3;
        private readonly List<IEquipment> _worn = new List<IEquipment>(Capacity);
        private readonly Hero _wearer;

        private const float Strength = 1f;

        // ====================== where the worn item sit on the hero ======================
        private const float RowY = -0.55f;
        private const float Spacing = 0.34f;
        private const float Scale = 0.3f;


        // ================================== getter ==================================
        public IReadOnlyList<IEquipment> Worn => _worn;
        public int Count => _worn.Count;
        public bool HasRoom => _worn.Count < Capacity;

        public HeroItems(Hero wearer)
        {
            _wearer = wearer;
        }

        // ================================== wear ==================================
        /// Put an item on wearer, in the free slot. 
        public bool TryWear(IEquipment item)
        {
            if (item == null || !HasRoom) return false;

            // add new item to the worn list
            _worn.Add(item);

            // parent item to wearer
            int slotPosition = _worn.Count - 1;
            Seat(item, slotPosition);

            // item give modifier to wearer
            Grant(item);

            return true;
        }

        // Hand the hero what the item gives.
        private void Grant(IEquipment item)
        {
            ICustomModifier granted = item.Modifier;

            if (granted == null) return;

            _wearer.AddModifier(granted, Strength, _wearer);
        }

        // Park the item under the hero, in its slot.
        private void Seat(IEquipment item, int slot)
        {
            Transform worn = item.transform;

            worn.SetParent(_wearer.transform, worldPositionStays: false);
            worn.localPosition = new Vector3((slot - 1) * Spacing, RowY, 0f);
            worn.localScale = new Vector3(Scale, Scale, 1f);

            // FIXLATER: later, the item should still have hitbox, to be inspectable.
            // FIXLATER: later2, you could drag the item out of hero.
            // A worn item is part of the hero now, and there is no taking one off yet, so it
            // stops being something the pointer can pick up or the drop can land on.
            Collider2D hitbox = worn.GetComponent<Collider2D>();
            if (hitbox != null) hitbox.enabled = false;

            DrawItemOverWearer(worn);
        }

        // make the item visiblity layer above hero.
        // so the item is not hide by he hero overlapping it.
        private void DrawItemOverWearer(Transform worn)
        {
            SpriteRenderer wornSprite = worn.GetComponent<SpriteRenderer>();
            SpriteRenderer wearerSprite = _wearer.GetComponent<SpriteRenderer>();
            if (wornSprite == null || wearerSprite == null) return;

            wornSprite.sortingLayerID = wearerSprite.sortingLayerID;
            wornSprite.sortingOrder = wearerSprite.sortingOrder + 1;
        }
    }
}
