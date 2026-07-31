using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ZoneAOE are legacy action that apply effect over time.
/// Effect here was apply to the recipients on standing in the effect, if they walk out of it, they don't get effect re-apply.
/// e.g. Garen, Silco, Swain
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ZoneAOE : LegacyAction
{
    [SerializeField] private float _lifetime = 0.5f;

    protected override float PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        Init(caster, effects, aimTargetPosition);

        return _castTime;
    }

    // ======================================== private ==============================================
    private void Init(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        // initialize local variable
        _caster = caster;
        _effects = effects;
        _lifetime = _castTime;

        // spawn cosmetic effect prefab, if any, and set this instance's lifetime
        if (_effectPrefab != null) Instantiate(_effectPrefab, aimTargetPosition, Quaternion.identity);
        Destroy(gameObject, _lifetime);

        // one hitbox for the whole zone - every cadence effect on it shares one interval
        OnTickHitbox hitbox = new OnTickHitbox();
        hitbox.OnHit += HandleTick;
        hitbox.Init(_caster);
        _hitbox = hitbox;

        // temporarily guard
        // all cadence effects on this zone must share one interval - guard against a design
        // mistake where they don't, since only the first interval found actually gets used
        float? interval = null;
        foreach (SkillEffect effect in _effects)
        {
            if (!effect.Cadence.isCadence) continue;

            if (interval == null) interval = effect.Cadence.cadenceInterval;
            else if (!Mathf.Approximately(interval.Value, effect.Cadence.cadenceInterval))
            {
                Debug.LogError($"{name}: cadence effects on the same zone must share one interval, found {interval.Value} and {effect.Cadence.cadenceInterval} - only {interval.Value} will be used", this);
            }
        }

        if (interval != null) StartCoroutine(CadenceTick(hitbox, interval.Value));
    }

    // a hero currently in the zone got ticked - apply every cadence effect to them
    private void HandleTick(Hero hero)
    {
        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            if (effect.Cadence.isCadence) ApplyEffectToRecipients(effect, recipients);
        }
    }

    // Global cadence tick - on a fixed schedule from spawn, no initial collision needed to start ticking.
    private IEnumerator CadenceTick(OnTickHitbox hitbox, float interval)
    {
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (true)
        {
            yield return wait;
            hitbox.FireTick();
        }
    }
}
