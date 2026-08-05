using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CircleAOE are legacy action that can apply effect "once" or "overtime".
/// 1) If apply once, it mean to apply effect to target immediately (apply at first contact).
///
/// 2) If apply overtime, it mean after first contact, it still apply damage over time to them afterward like a poison.
/// Herores who get poison will can't walk out of poison like ZoneAOE, but they get full damage duration instead.
/// 
/// Example
/// e.g. Teemo
/// </summary>
public class CircleAOE : AOE
{
    private float _lifetime = 0.5f;

    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();

    // ======================================== private ==============================================
    protected override void PlayLegacyAction()
    {
        // initialize local variable
        _lifetime = _castTime;

        // set object lifetime
        Destroy(gameObject, _lifetime);

        // initialize hitbox: dispatch once-or-cadence per hero, on their first contact only
        OnContactHitbox onceHitbox = new OnContactHitbox();
        onceHitbox.OnHit += HandleFirstHit;
        _hitbox = onceHitbox;
        _hitbox.Init(_me);
    }

    /// <summary>
    /// When a hero who was hit on first contact:
    /// 1) Apply effect once if not cadence
    /// 2) Apply effect over time if cadence
    /// </summary>
    private void HandleFirstHit(Hero hero)
    {
        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            // if not cadence, apply once
            if (!effect.Cadence.isCadence)
            {
                ApplyEffectToRecipients(effect, recipients);
            }

            // if cadence true, apply effect multiple time
            else if (_triggeredOnce.Add((effect, hero)))
            {
                // run coroutine on target hero to apply that effect 
                hero.StartCoroutine(PerHeroCadenceTick(effect, hero));
            }
        }
    }

    // This is for the poison DOT. I think we should move this to its own class e.g. Status class later.
    // Per hero cadence tick
    // later should be change to poison status or something like that
    private IEnumerator PerHeroCadenceTick(SkillEffect effect, Hero hero)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
        List<Hero> recipients = new List<Hero> { hero };
        float elapsed = 0f;

        while (elapsed < effect.Cadence.cadenceDuration)
        {
            yield return wait;
            elapsed += effect.Cadence.cadenceInterval;

            if (hero == null || hero.State == HeroStateType.Dead) yield break;
            ApplyEffectToRecipients(effect, recipients);
        }
    }
}