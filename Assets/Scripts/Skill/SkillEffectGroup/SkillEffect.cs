using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Not cadence => apply effect when hitbox collide (OnTriggerEnter())
    /// cadence but start only initial collision => e.g. teemo's mushroom (after OnTriggerEnter(), start coroutine for the poison damage)
    /// cadence but not need initial collision => e.g. garen's E (OnTriggerEnter() trigger every interval)
    /// </summary>
    [Serializable]
    public class Cadence
    {
        public bool isCadence = false;

        // neither is read unless isCadence, so neither is shown unless isCadence
        [ShowIf(nameof(isCadence))] public float cadenceInterval = 0.5f;    // the interval of time effect is re-apply
        [ShowIf(nameof(isCadence))] public float cadenceDuration = 3f;      // total duration of the effect
    }

    [Serializable]
    public abstract class SkillEffect
    {
        [SerializeField] protected EffectRecipientEnum _recipient;        // the list of recipients who get effect
        [SerializeField] protected Cadence _cadence = new Cadence();      // Is effect reapply over time?
        [SerializeReference] protected SkillCondition _condition;         // condition for this effect. => if satisfied, effect is applified.
        // nothing amplifies without a condition to amplify on, so this stays hidden until there is one
        [ShowIf(nameof(_condition))]
        [SerializeField] protected float _amplifier = 0.3f;               // e.g. 0.3 = +30% when the condition holds

        // ================================== getter ==================================
        public EffectRecipientEnum Recipient => _recipient;
        public Cadence Cadence => _cadence;
        public SkillCondition Condition => _condition;

        // check condition to each recipients. 
        // If condition is met, the effect is amplified.
        protected float AmplifierFor(IDamageable caster, IDamageable recipient)
        {
            if (SkillCondition.Ask(_condition, caster, recipient) == ConditionResultEnum.ConditionIsMet)
            {
                return 1f + _amplifier;
            }

            return 1f;
        }

        public abstract void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients);
    }

    [Serializable]
    public class AttackSkillEffect : SkillEffect
    {
        [SerializeField] private float _damageAmount;

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                // if the damage was cadence, calculate tick damage instead
                float dmg;
                if (_cadence.isCadence) dmg = _damageAmount * _cadence.cadenceInterval / _cadence.cadenceDuration;

                // if not cadence, apply normal flat damage
                else dmg = _damageAmount;

                // if pass specify condition, amplify the effect
                dmg *= AmplifierFor(caster, recipient);

                // apply damage
                recipient.TakeDamage(Mathf.RoundToInt(dmg));
            }
        }

    }

    // FIXLATER: I don't like that we have several skilleffect BUT BuffSkillEffect/DebuffSkillEffect/StatusSkillEffect does the same thing.
    // Even Healskilleffect is also lumpable into those too. Let's leave it for now, let consider this again when we see pattern more clearly.
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

    [Serializable]
    public class BuffSkillEffect : SkillEffect, Modifier
    {
        [SerializeField] private ModifierEnum _modifier;
        [SerializeField] private float _buffAmount;
        [SerializeField] private float _buffDuration;

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                recipient.AddModifier(this);
            }
        }

        public float GetAmount()
        {
            return _buffAmount;
        }

        public ModifierEnum GetModifierEnum()
        {
            return _modifier;
        }

        public float GetDuration()
        {
            return _buffDuration;
        }
    }

    [Serializable]
    public class DebuffSkillEffect : SkillEffect, Modifier
    {
        [SerializeField] private ModifierEnum _modifier;
        [SerializeField] private float _debuffAmount;
        [SerializeField] private float _deBuffDuration;

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                recipient.AddModifier(this);
            }
        }

        public float GetAmount()
        {
            return _debuffAmount;
        }

        public ModifierEnum GetModifierEnum()
        {
            return _modifier;
        }

        public float GetDuration()
        {
            return _deBuffDuration;
        }
    }

    [Serializable]
    public class StatusSkillEffect : SkillEffect, Modifier
    {
        [SerializeField] private ModifierEnum _modifier;
        [SerializeField] private float _statusAmount;
        [SerializeField] private float _statusDuration;

        public override void ApplyEffect(IDamageable caster, IReadOnlyList<IDamageable> recipients)
        {
            foreach (IDamageable recipient in recipients)
            {
                if (recipient == null || !recipient.IsAlive) continue;

                recipient.AddModifier(this);
            }
        }

        public float GetAmount()
        {
            return _statusAmount;
        }

        public ModifierEnum GetModifierEnum()
        {
            return _modifier;
        }

        public float GetDuration()
        {
            return _statusDuration;
        }
    }
}
