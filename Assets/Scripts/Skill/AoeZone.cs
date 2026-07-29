using System.Collections.Generic;
using UnityEngine;

// Lives on the spawned AOE effect prefab. Detects enemy heroes overlapping its trigger collider
// and applies each area effect to them. Requires a Rigidbody2D on this object since Hero's own
// collider carries none - Unity's 2D trigger events need at least one side of the pair to have one.
[RequireComponent(typeof(Rigidbody2D))]
public class AoeZone : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    private Hero _caster;
    private List<SkillEffect> _effects;

    // Heroes currently standing in the zone - kept in sync via enter/exit so cadence effects
    // know who to re-apply to without re-querying physics every tick.
    private readonly List<Hero> _heroesInZone = new List<Hero>();
    private readonly Dictionary<SkillEffect, float> _cadenceTimers = new Dictionary<SkillEffect, float>();

    public void Init(Hero caster, List<SkillEffect> effects, float castTime)
    {
        _caster = caster;
        _effects = effects;
        _lifetime = castTime;
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        _heroesInZone.RemoveAll(hero => hero == null || hero.State == HeroStateType.Dead);
        if (_heroesInZone.Count == 0) return;

        // apply effect every cadence interval
        foreach (SkillEffect effect in _effects)
        {
            if (!effect.Cadence.isCadence) continue;

            // get timer for current effect
            float timer = _cadenceTimers.TryGetValue(effect, out float t) ? t : 0f;
            
            // update timer 
            timer += Time.deltaTime;

            // apply effect every interval
            if (timer >= effect.Cadence.cadenceInterval)
            {
                timer -= effect.Cadence.cadenceInterval;
                ApplyEffectToRecipients(effect, _heroesInZone);
            }

            // update timer in dictionary
            _cadenceTimers[effect] = timer;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();

        // not apply effect to myself, my team, the dead hero
        if (hero == null || hero.Team == _caster.Team || hero.State == HeroStateType.Dead) return;

        if (!_heroesInZone.Contains(hero)) _heroesInZone.Add(hero);

        // // apply effect to all hit hero
        // List<Hero> recipients = new List<Hero> { hero };
        // foreach (SkillEffect effect in _effects)
        // {
        //     ApplyEffectToRecipients(effect, recipients);
        // }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();
        if (hero != null) _heroesInZone.Remove(hero);
    }

    private void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _caster });

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
    }
}
