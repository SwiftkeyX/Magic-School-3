using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    // FLAGGING: I don't sure if it was generic enough to have a inheritance. Let see laterward.
    // effect that apply heal to recipients
    [Serializable]
    public class HealSkillEffect : SkillEffect
    {
        [SerializeField] private float _totalHealAmount;   // spread evenly across every cadence tick over _duration
        [SerializeField] private float _duration;

        public float Duration => _duration;

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            int totalTicks = Mathf.Max(1, Mathf.RoundToInt(_duration / Cadence.cadenceInterval));
            float healPerTick = _totalHealAmount / totalTicks;

            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                recipient.Heal(healPerTick * AmplifierFor(caster, recipient));
            }
        }
    }
}
