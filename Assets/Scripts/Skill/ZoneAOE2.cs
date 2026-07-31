using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ZoneAOE are legacy action that apply effect over time.
/// Effect here was apply to the recipients on standing in the effect, if they walk out of it, they don't get effect re-apply.
/// e.g. Garen, Silco, Swain
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ZoneAOE2 : LegacyAction
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

        // spawn prefab, and set prefab lifetime
        Instantiate(_effectPrefab, aimTargetPosition, Quaternion.identity);
        Destroy(gameObject, _lifetime);

        // Immediately start a coroutine for apply effect over time
        foreach (SkillEffect effect in _effects)
        {
            if (effect.Cadence.isCadence) StartCoroutine(CadenceTick(effect));
        }

        // initialize hitbox
        _hitbox.Init(_caster);
    }

    // Global cadence tick
    // on a fixed schedule from spawn - no initial collision needed to start ticking.
    private IEnumerator CadenceTick(SkillEffect effect)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);

        while (true)
        {
            yield return wait;

            // remove hero that are null & dead
            _hitbox.GetRecipients().RemoveAll(hero => hero == null || hero.State == HeroStateType.Dead);

            // apply effect to recipient
            bool isAnyoneHit = _hitbox.GetRecipients().Count > 0;
            if (isAnyoneHit) ApplyEffectToRecipients(effect, _hitbox.GetRecipients());
        }
    }
}
