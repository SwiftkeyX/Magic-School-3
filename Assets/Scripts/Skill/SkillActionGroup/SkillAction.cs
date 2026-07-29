using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillActionGroup
{
    [SerializeField] private ActionSource _source;
    [SerializeField] private LegacyAction _legacyAction;
    [SerializeField] private AimTarget _target;
    [SerializeField] private HitboxSize _size = new HitboxSize();

    // public Offset _offset;
    // ...

    [SerializeReference] private List<SkillEffect> _effects;    // 1 action = have several effect

    // ============================================ Getter ============================================
    public ActionSource Source => _source;
    public LegacyAction LegacyAction => _legacyAction;
    public AimTarget Target => _target;
    public HitboxSize Size => _size;
    public List<SkillEffect> Effects => _effects;
}

