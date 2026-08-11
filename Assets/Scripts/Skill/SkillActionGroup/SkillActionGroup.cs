using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    [Serializable]
    public abstract class SkillActionGroup
    {
        [SerializeField] private ActionSourceEnum _source;
        [SerializeField] private TemplateAction _templateAction;
        [SerializeField] private AimTargetEnum _target;
        [SerializeField] private HitboxSize _size = new HitboxSize();

        // public Offset _offset;
        // ...

        [SerializeReference] private List<SkillEffect> _effects;    // 1 template action = have several effect

        // ============================================ Getter ============================================
        public ActionSourceEnum Source => _source;
        public TemplateAction TemplateAction => _templateAction;
        public AimTargetEnum Target => _target;
        public HitboxSize Size => _size;
        public List<SkillEffect> Effects => _effects;
    }

    [Serializable]
    public class SkillActiveGroup : SkillActionGroup { }

    [Serializable]
    public class SkillPassiveGroup : SkillActionGroup { }
}
