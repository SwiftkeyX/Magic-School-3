using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the spawned AOE effect prefab. Unlike ZoneAOE (which ticks cadence effects globally
// against whoever's currently inside), CircleAOE's cadence effects are triggered once by initial
// contact and then keep re-applying to that specific hero regardless of position - e.g. Teemo's
// mushroom. Requires a Rigidbody2D on this object since Hero's own collider carries none - Unity's
// 2D trigger events need at least one side of the pair to have one.
[RequireComponent(typeof(Rigidbody2D))]
public class CircleAOE : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    private Hero _caster;
    private List<SkillEffect> _effects;

    // Guards against re-triggering a second DoT coroutine on the same hero if they leave and
    // re-enter the hitbox while it's still alive.
    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();

    public void Init(Hero caster, List<SkillEffect> effects, float castTime)
    {
        _caster = caster;
        _effects = effects;
        _lifetime = castTime;
        Destroy(gameObject, _lifetime);
    }

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
            else if (_triggeredOnce.Add((effect, hero)))
            {
                // e.g. Teemo's mushroom - contact triggers a DoT that keeps ticking on this
                // specific hero regardless of whether they stay standing in the hitbox
                StartCoroutine(PerHeroCadenceTick(effect, hero));
            }
        }
    }

    // Per-hero cadence tick e.g. Teemo's mushroom: triggered once by contact, then keeps
    // re-applying to that specific hero even after they leave the hitbox.
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
