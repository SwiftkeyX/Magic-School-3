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
[RequireComponent(typeof(Rigidbody2D))]
public class CircleAOE : LegacyAction
{
    [SerializeField] private float _lifetime = 0.5f;

    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();

    // ======================================== private ==============================================
    protected override void PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        // initialize local variable
        _caster = caster;
        _effects = effects;
        _lifetime = _castTime;

        // set object lifetime
        Destroy(gameObject, _lifetime);

        // initialize hitbox: dispatch once-or-cadence per hero, on their first contact only
        OnContactHitbox onceHitbox = new OnContactHitbox();
        onceHitbox.OnHit += HandleFirstHit;
        _hitbox = onceHitbox;
        _hitbox.Init(_caster);
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
            if (!effect.Cadence.isCadence)
            {
                ApplyEffectToRecipients(effect, recipients);
            }
            else if (_triggeredOnce.Add((effect, hero)))
            {
                StartCoroutine(PerHeroCadenceTick(effect, hero));
            }
        }
    }

    // Per hero cadence tick
    // later should be change to poison status or something like that
    private IEnumerator PerHeroCadenceTick(SkillEffect effect, Hero hero)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
        List<Hero> recipients = new List<Hero> { hero };

        while (true)
        {
            yield return wait;

            if (hero == null || hero.State == HeroStateType.Dead) yield break;
            ApplyEffectToRecipients(effect, recipients);
        }
    }
}