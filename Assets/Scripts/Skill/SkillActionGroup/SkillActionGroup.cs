using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    [Serializable]
    public class SkillActionGroup
    {
        [SerializeField] private ActionSourceEnum _source;
        [SerializeField] private TemplateAction _templateAction;
        [SerializeField] private AimTargetEnum _target;
        [SerializeReference] private List<SkillCondition> _conditions;

        // public Offset _offset;
        // ...

        [SerializeReference] private List<SkillEffect> _effects;    // 1 template action = have several effect

        // ============================================ Getter ============================================
        public ActionSourceEnum Source => _source;
        public TemplateAction TemplateAction => _templateAction;
        public AimTargetEnum Target => _target;
        public List<SkillCondition> Conditions => _conditions;
        public List<SkillEffect> Effects => _effects;
    }
}
