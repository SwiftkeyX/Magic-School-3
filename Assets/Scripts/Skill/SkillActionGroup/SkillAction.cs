using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillActionGroup
{
    public ActionSource _source;
    public ActionName _actionName;
    public AimTarget _target;
    public AOE _aoe;

    // public Offset _offset;
    // ...

    [SerializeField] private List<SkillEffect> _effects;    // 1 action = have several effect
}

