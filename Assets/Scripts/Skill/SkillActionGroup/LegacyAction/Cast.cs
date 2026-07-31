using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cast is the legacy action for instant self-effects (e.g. self-buffs).
/// No hitbox, no physical footprint - it applies its effects to the caster and is done.
/// e.g. Galio's Idol of Durand step 1.
/// </summary>
public class Cast : LegacyAction
{
    protected override void PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        _caster = caster;
        List<Hero> self = new List<Hero> { caster };

        bool hasCadenceEffect = false;
        foreach (SkillEffect effect in effects)
        {
            if (effect.Recipient != EffectRecipientEnum.Self) continue;

            // Cast has no physical hitbox/lifetime to bound ticking, so a cadence effect here
            // needs its own duration - only HealSkillEffect does that today.
            if (effect.Cadence.isCadence && effect is HealSkillEffect healEffect)
            {
                hasCadenceEffect = true;
                StartCoroutine(CadenceTick(healEffect, self));
            }
            else
            {
                effect.ApplyEffect(self);
            }
        }

        // one-shot effects are already applied - only stick around if a cadence effect needs to keep ticking
        if (!hasCadenceEffect) Destroy(gameObject);
    }

    private IEnumerator CadenceTick(HealSkillEffect effect, List<Hero> recipients)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
        float elapsed = 0f;

        while (elapsed < effect.Duration)
        {
            yield return wait;
            elapsed += effect.Cadence.cadenceInterval;

            effect.ApplyEffect(recipients);
        }

        Destroy(gameObject);
    }
}
