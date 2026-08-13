using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Cast is the template action for instant self-effects (e.g. self-buffs).
    /// No hitbox, no physical footprint - it applies its effects to the caster and is done.
    /// e.g. Galio's Idol of Durand step 1.
    /// </summary>
    public class Cast : TemplateAction
    {
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            _source = _me.transform.position;
            return true;
        }

        protected override bool ResolveAimTarget(AimTargetEnum aimTarget) => true;

        protected override Vector3 GetSpawnPosition() => _source;

        protected override void Play()
        {
            // initialize local variable
            // FLAGGING: Cast always applies to the caster (for now)
            List<Hero> self = new List<Hero> { _me };

            bool hasCadenceEffect = false;
            foreach (SkillEffect effect in _effects)
            {
                if (effect.Recipient != EffectRecipientEnum.Self) continue;

                // if cadence, apply effect over time. 
                // e.g. galio heal
                if (effect.Cadence.isCadence && effect is HealSkillEffect healEffect)
                {
                    hasCadenceEffect = true;
                    StartCoroutine(CadenceTick(healEffect, self));
                }

                // if not cadence, apply effect once.
                else
                {
                    effect.ApplyEffect(self);
                }
            }

            // temp
            if (!hasCadenceEffect) DestroyMe();
        }
    }
}
