using System.Collections.Generic;
using UnityEngine;

// /// <summary>
// /// ZoneAOE, CircleAOE, etc... are all legacy action.
// /// later LegacyAction should be interface instead.
// /// </summary>
// [CreateAssetMenu(fileName = "LegacyAction", menuName = "Magic School 3/Legacy Action")]
// public class LegacyAction : ScriptableObject
// {
//     [SerializeField] private LegacyActionEnum _actionName;
//     [SerializeField] private GameObject _effect;    // the prefab for the skill
//     [SerializeField] private float _castTime;       // the duration skill was cast

//     // ==================================== public method ====================================
//     // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
//     // that'll be resolve later
//     public float TriggerSkill(AimTarget aimTarget, Hero caster, List<SkillEffect> effects)
//     {
//         // find position using aim target
//         Vector3 aimTargetPosition = ResolveAimTarget(aimTarget, caster);

//         // play animation based on legacy action
//         // return cast duration
//         return PlayLegacyAction(caster, effects, aimTargetPosition);
//     }

//     // ==================================== LegacyAction ====================================
//     private float PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
//     {
//         if (_actionName == LegacyActionEnum.ZoneAOE)
//         {
//             SpawnZoneAOE(caster, effects, aimTargetPosition);
//         }

//         else if (_actionName == LegacyActionEnum.CircleAOE)
//         {
//             SpawnCircleAOE(caster, effects, aimTargetPosition);
//         }

//         else if (_actionName == LegacyActionEnum.Cast)
//         {
//             Cast(caster, effects);
//         }

//         return _castTime;
//     }

//     private void SpawnZoneAOE(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
//     {
//         if (_effect == null) return;

//         GameObject instance = Object.Instantiate(_effect, aimTargetPosition, Quaternion.identity);
//         // assign prefab data at runtime
//         ZoneAOE zone = instance.GetComponent<ZoneAOE>();
//         if (zone != null) zone.Init(caster, effects, _castTime);
//     }

//     private void SpawnCircleAOE(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
//     {
//         if (_effect == null) return;

//         GameObject instance = Object.Instantiate(_effect, aimTargetPosition, Quaternion.identity);
//         CircleAOE circle = instance.GetComponent<CircleAOE>();
//         if (circle != null) circle.Init(caster, effects, _castTime);
//     }

//     // Cast are basically the custom legacy action
//     // it does variety of thing. 
//     // NOTE that now it only use to buff.
//     private void Cast(Hero caster, List<SkillEffect> effects)
//     {
//         List<Hero> self = new List<Hero> { caster };
//         foreach (SkillEffect effect in effects)
//         {
//             if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(self);
//         }
//     }

//     // ==================================== etc ====================================
//     private Vector3 ResolveAimTarget(AimTarget aimTarget, Hero caster)
//     {
//         // find position using aim target
//         if (aimTarget == AimTarget.Self)
//         {
//             return caster.transform.position;
//         }

//         return Vector3.zero;
//     }
// }

public abstract class LegacyAction : MonoBehaviour
{
    [SerializeField] private LegacyActionEnum _actionName;
    [SerializeField] protected GameObject _effectPrefab;    // the prefab for the skill
    [SerializeField] protected float _castTime;       // the duration skill was cast
    protected Hero _caster;
    protected List<SkillEffect> _effects;
    protected Hitbox _hitbox;

    // ==================================== public method ====================================
    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public virtual float TriggerSkill(AimTarget aimTarget, Hero caster, List<SkillEffect> effects)
    {
        // find position using aim target
        Vector3 aimTargetPosition = ResolveAimTarget(aimTarget, caster);

        // _legacyAction fields reference a prefab asset, not a scene instance - spawn a real
        // clone to act on so physics/Destroy() have an actual live GameObject to work with.
        LegacyAction instance = Instantiate(this, aimTargetPosition, Quaternion.identity);

        // play animation based on legacy action
        // return cast duration
        return instance.PlayLegacyAction(caster, effects, aimTargetPosition);
    }

    // ==================================== local method ====================================
    private Vector3 ResolveAimTarget(AimTarget aimTarget, Hero caster)
    {
        // find position using aim target
        if (aimTarget == AimTarget.Self)
        {
            return caster.transform.position;
        }

        return Vector3.zero;
    }

    // ==================================== protected method ====================================
    protected abstract float PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition);

    protected void OnTriggerEnter2D(Collider2D other) => _hitbox.OnTriggerEnter2D(other);

    protected void OnTriggerExit2D(Collider2D other) => _hitbox.OnTriggerExit2D(other);

    // ==================================== Effect & Recipient ====================================
    // apply effect to the recipients
    protected void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _caster });

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
    }
}

