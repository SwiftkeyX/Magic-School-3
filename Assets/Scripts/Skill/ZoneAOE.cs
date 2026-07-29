using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ZoneAOE are legacy action that apply effect over time.
/// Effect here was apply to the recipients on standing in the effect, if they walk out of it, they don't get effect re-apply.
/// e.g. Garen, Silco, Swain
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ZoneAOE : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    private Hero _caster;
    private List<SkillEffect> _effects;

    private readonly List<Hero> _heroesWhoWasHit = new List<Hero>();

    public void Init(Hero caster, List<SkillEffect> effects, float castTime)
    {
        _caster = caster;
        _effects = effects;
        _lifetime = castTime;
        Destroy(gameObject, _lifetime);

        // Immediately start a coroutine for apply effect over time
        foreach (SkillEffect effect in _effects)
        {
            if (effect.Cadence.isCadence) StartCoroutine(CadenceTick(effect));
        }
    }

    // Get all the hero who was hit by the skill
    private void OnTriggerEnter2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();

        // not apply effect to myself, my team, the dead hero
        if (hero == null || hero.Team == _caster.Team || hero.State == HeroStateType.Dead) return;

        // Group all the heroes who was hit by the skill in 1 list 
        if (!_heroesWhoWasHit.Contains(hero)) _heroesWhoWasHit.Add(hero);
    }

    // Remove hero who was not hit by the skill out of the list
    private void OnTriggerExit2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();
        if (hero != null) _heroesWhoWasHit.Remove(hero);
    }

    // Global cadence tick
    // on a fixed schedule from spawn - no initial collision needed to start ticking.
    private IEnumerator CadenceTick(SkillEffect effect)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);

        while (true)
        {
            yield return wait;

            _heroesWhoWasHit.RemoveAll(hero => hero == null || hero.State == HeroStateType.Dead);
            if (_heroesWhoWasHit.Count > 0) ApplyEffectToRecipients(effect, _heroesWhoWasHit);
        }
    }

    // apply effect to the recipients
    private void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _caster });

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
    }
}
