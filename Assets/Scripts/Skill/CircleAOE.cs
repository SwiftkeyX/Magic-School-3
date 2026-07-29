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
public class CircleAOE : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    private Hero _caster;
    private List<SkillEffect> _effects;
    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();  

    public void Init(Hero caster, List<SkillEffect> effects, float castTime)
    {
        _caster = caster;
        _effects = effects;
        _lifetime = castTime;
        Destroy(gameObject, _lifetime);
    }

    /// <summary>
    /// When heroes who was hit on first contact:
    /// 1) Apply effect once if not cadence
    /// 2) Apply effect over time if cadence
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_caster == null) return;

        Hero hero = other.GetComponent<Hero>();

        // not apply effect to myself, my team, the dead hero
        if (hero == null || hero.Team == _caster.Team || hero.State == HeroStateType.Dead) return;

        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            // apply once
            if (!effect.Cadence.isCadence)
            {
                // apply effect once
                ApplyEffectToRecipients(effect, recipients);
            }

            // apply over time BUT only apply to target who wasn't hit already 
            // (prevent hitting twice)
            else if (_triggeredOnce.Add((effect, hero)))
            {
                // apply effect over time
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

    private void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _caster });

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
    }
}
