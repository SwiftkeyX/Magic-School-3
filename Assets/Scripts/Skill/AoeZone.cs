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

    public void Init(Hero caster, List<SkillEffect> effects)
    {
        _caster = caster;
        _effects = effects;
        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Hero hero = other.GetComponent<Hero>();
        if (hero == null || hero.Team == _caster.Team || hero.State == HeroStateType.Dead) return;

        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            effect.ApplyEffect(recipients);
        }
    }
}
