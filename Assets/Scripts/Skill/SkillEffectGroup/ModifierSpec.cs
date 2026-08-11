using System;
using UnityEngine;

namespace MagicSchool
{
    // The actual modifier class 
    // To contain modifier type, its amount, its duration in 1 place
    // FLAGGING: We use to need IModifier, but it don't neccessary anymore, since ModifierSpec is the only user. 
    // Let leave it for now though, maybe it'll see more used?.
    [Serializable]
    public class ModifierSpec : IModifier
    {
        [SerializeField] private ModifierEnum _modifier;
        [SerializeField] private float _amount;
        [SerializeField] private float _duration;

        public ModifierEnum GetModifierEnum() => _modifier;
        public float GetAmount() => _amount;
        public float GetDuration() => _duration;

        public IModifier Scaled(float multiplier)
        {
            // scale amount base on multiplier
            _amount *= multiplier;

            // return self
            return this;
        }
    }
}