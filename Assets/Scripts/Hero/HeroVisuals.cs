using UnityEngine;

/// <summary>
/// The presentation layer for hero - as opposed to data layer (HeroDataRuntime) or logic layer (the states).
/// This is still unstable though. I think it would be changed a lot from now.
/// </summary>
// FIXLATER: this wants to be a MonoBehaviour on the hero prefab, so the cast-text prefab is
// wired in the Inspector instead of pulled from Resources (a legacy API). Held off because
// all 19 hero prefabs are standalone rather than variants of BaseHero, so it'd mean adding
// and wiring the component 19 times - and missing one only shows up as a null ref at runtime.
// Worth doing together with making those prefabs variants.
public class HeroVisuals
{
    private const float DeadAlpha = 0.3f;

    private static readonly Color BlueSkillTextColor = new Color(0.4f, 0.85f, 1f);
    private static readonly Color RedSkillTextColor = new Color(1f, 0.55f, 0.3f);

    // Shared across heroes on purpose - it's one immutable prefab reference, not per-hero state.
    private static GameObject _skillCastTextPrefab;

    private readonly Hero _me;
    private readonly SpriteRenderer _sprite;

    public HeroVisuals(Hero hero, SpriteRenderer sprite)
    {
        _me = hero;
        _sprite = sprite;
    }

    // When hero die, set his sprite transparent to indicate that he is dead.
    public void SetDeadVisual()
    {
        Color c = _sprite.color;
        c.a = DeadAlpha;
        _sprite.color = c;
    }

    // Floating skill-name text above the hero when its skill actually casts
    public void PlaySkillCastEffect(string skillName)
    {
        if (_skillCastTextPrefab == null) _skillCastTextPrefab = Resources.Load<GameObject>("VFX/SkillCastText");
        if (_skillCastTextPrefab == null) return;

        GameObject instance = Object.Instantiate(_skillCastTextPrefab);
        Color color = _me.Team == Team.Blue ? BlueSkillTextColor : RedSkillTextColor;
        instance.GetComponent<FloatingText>().Show(skillName, color, _me.transform.position);
    }
}
