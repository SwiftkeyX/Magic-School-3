using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the spawned AOE effect prefab. Detects enemy heroes overlapping its trigger collider
// and applies each area effect to them. Requires a Rigidbody2D on this object since Hero's own
// collider carries none - Unity's 2D trigger events need at least one side of the pair to have one.
[RequireComponent(typeof(Rigidbody2D))]
public class ZoneAOE : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    private Hero _caster;
    private List<SkillEffect> _effects;

    // Heroes currently standing in the zone - kept in sync via enter/exit so globally-ticking
    // cadence effects know who to re-apply to without re-querying physics every tick.
    private readonly List<Hero> _heroesWhoWasHit = new List<Hero>();

    // Guards startOnInitialCollisionOnly effects from re-triggering a second DoT coroutine on the
    // same hero if they leave and re-enter the zone while it's still alive.
    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();

    public void Init(Hero caster, List<SkillEffect> effects, float castTime)
    {
        _caster = caster;
        _effects = effects;
        _lifetime = castTime;
        Destroy(gameObject, _lifetime);

        // Cadence effects that don't need an initial collision tick globally from spawn,
        // applying to whoever's inside at each interval e.g. Garen's E.
        foreach (SkillEffect effect in _effects)
        {
            if (effect.Cadence.isCadence && !effect.Cadence.startOnInitialCollisionOnly)
            {
                StartCoroutine(CadenceTick(effect));
            }
        }
    }

    // Get all the hero who was hit by the skill
    private void OnTriggerEnter2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();

        // not apply effect to myself, my team, the dead hero
        if (hero == null || hero.Team == _caster.Team || hero.State == HeroStateType.Dead) return;

        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            if (!effect.Cadence.isCadence)
            {
                // apply effect once, immediately on contact
                ApplyEffectToRecipients(effect, recipients);
            }
            else if (effect.Cadence.startOnInitialCollisionOnly && _triggeredOnce.Add((effect, hero)))
            {
                // e.g. Teemo's mushroom - contact triggers a DoT that keeps ticking on this
                // specific hero regardless of whether they stay standing in the zone
                StartCoroutine(PerHeroCadenceTick(effect, hero));
            }
        }

        if (!_heroesWhoWasHit.Contains(hero)) _heroesWhoWasHit.Add(hero);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();
        if (hero != null) _heroesWhoWasHit.Remove(hero);
    }

    // Global cadence tick e.g. Garen's E: applies to whoever is currently standing in the zone,
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

    // Per-hero cadence tick e.g. Teemo's mushroom: triggered once by contact, then keeps
    // re-applying to that specific hero even after they leave the zone.
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
