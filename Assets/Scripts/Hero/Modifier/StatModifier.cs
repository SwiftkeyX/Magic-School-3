// using System;
// using System.Collections.Generic;


// /// <summary>
// /// SRP = Update, Sum, Has 
// /// </summary>
// public class ModifierList
// {
//     private readonly List<StatModifier> _modifiers = new List<StatModifier>();

//     public void Add(StatModifier modifier) => _modifiers.Add(modifier);

//     public float Sum(SkillEffect detail)
//     {
//         float sum = 0f;
//         foreach (StatModifier modifier in _modifiers)
//             if (modifier.Detail == detail) sum += modifier.Amount;
//         return sum;
//     }

//     public bool Has(SkillEffect detail)
//     {
//         foreach (StatModifier modifier in _modifiers)
//             if (modifier.Detail == detail) return true;
//         return false;
//     }

//     public void Tick(float deltaTime, Action<StatModifier> onExpired)
//     {
//         for (int i = _modifiers.Count - 1; i >= 0; i--)
//         {
//             StatModifier modifier = _modifiers[i];
//             if (float.IsPositiveInfinity(modifier.Remaining)) continue;

//             modifier.Remaining -= deltaTime;
//             if (modifier.Remaining <= 0f)
//             {
//                 _modifiers.RemoveAt(i);
//                 onExpired?.Invoke(modifier);
//             }
//         }
//     }
// }


// // StatModifier was used by ModifierList to calculate thing:
// // 1) One timed buff/debuff instance layered on top of a Stat's base values.
// // 2) Amount's meaning depends on Detail 
// // (e.g. BonusHP/AttackSpeed/DamageReduction add stat, Stun/Wound is a boolean which don't need amount)
// // 3) Remaining counts down in TickModifiers;
// // 4) float.PositiveInfinity means "lasts until something else removes it" (the sheet's "Permanent").
// public class StatModifier
// {
//     public readonly SkillEffect Detail;
//     public readonly float Amount;
//     public float Remaining;

//     public StatModifier(SkillEffect detail, float amount, float durationSeconds)
//     {
//         Detail = detail;
//         Amount = amount;
//         Remaining = durationSeconds <= 0f ? float.PositiveInfinity : durationSeconds;
//     }
// }

