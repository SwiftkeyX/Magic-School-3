using System.Collections.Generic;
using UnityEngine;


public abstract class LegacyAction : MonoBehaviour
{
    [SerializeField] private LegacyActionEnum _actionName;
    [SerializeField] protected float _castTime;       // the duration skill was cast
    protected Hero _caster;
    protected List<SkillEffect> _effects;
    protected Hitbox _hitbox;

    // ==================================== public method ====================================
    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public float TriggerSkill(AimTarget aimTarget, Hero caster, List<SkillEffect> effects)
    {
        // find position using aim target
        Vector3 aimTargetPosition = ResolveAimTarget(aimTarget, caster);

        // Instantiate the LegacyAction BC the current instance is the prefab version 
        // which can't interact with Unity's physics e.g. OnTriggerEnter() 
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

