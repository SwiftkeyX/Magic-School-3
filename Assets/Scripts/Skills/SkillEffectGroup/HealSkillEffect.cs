using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // FLAGGING: Try combine into ModifierSkillEffect. Not working, since the ModifierSkillEffect already work well.
    // It still look weird to me but it work well and clean. Lets leave it.
    public class HealSkillEffect : SkillEffect
    {
        private float _totalHealAmount;   // spread evenly across every cadence tick over _duration
        private float _duration;

        public float Duration => _duration;

        public HealSkillEffect(EffectRecipientEnum recipient, float totalHealAmount, float duration = -1f,
                               Cadence cadence = null, List<SkillCondition> conditions = null, float amplifier = 0.3f)
            : base(recipient, cadence, conditions, amplifier)
        {
            _totalHealAmount = totalHealAmount;
            _duration = duration;
        }

        // heal the reciepient
        // if the heal was cadence, it only heal the divided amount each time.
        public override void ApplyEffect(IReadOnlyList<IEffectable> recipients)
        {
            int totalTicks = Mathf.Max(1, Mathf.RoundToInt(_duration / Cadence.cadenceInterval));
            float healPerTick = _totalHealAmount / totalTicks;

            foreach (IEffectable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                recipient.Heal(healPerTick * AmplifierFor(recipient));
            }
        }
    }
}
