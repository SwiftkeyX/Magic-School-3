using System;
using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Items
{
    // this is where the item was registered in the game.
    public static class ItemLibrary
    {
        // a pair of ItemIdEnum & Modifiers
        private static readonly Dictionary<ItemIdEnum, Func<ICustomModifier>> Builders =
            new Dictionary<ItemIdEnum, Func<ICustomModifier>>
            {
                { ItemIdEnum.IronPlate, IronPlate.Build },
            };

        /// Return a modifier that match itemID.
        public static ICustomModifier Resolve(ItemIdEnum itemId)
        {
            if (itemId == ItemIdEnum.None) return null;

            if (!Builders.TryGetValue(itemId, out var build))
            {
                Debug.LogError($"[ItemLibrary] no builder for {itemId}. The item will grant nothing.");
                return null;
            }

            return build();
        }
    }
}
