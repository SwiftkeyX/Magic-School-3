using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// What the step before this one produced, handed to the step it triggers.
    ///
    /// One object instead of a parameter per trigger: 
    /// OnHit knows a position, 
    /// OnKill will know who died, 
    /// OnAttack will know who was being attacked. 
    /// BLOCKED: add context for OnKill, OnAttack later
    ///
    /// Every field is optional - a trigger fills in only what it actually knows.
    /// </summary>
    public class SkillStepContext
    {
        // ================================== Recipient ==================================
        public IDamageable Me { get; private set; }
        public IDamageable Recipient { get; private set; }

        // ================================== Template action ==================================
        public Vector3? Position { get; private set; }  // projectile hit position
        // ...

        // ================================== Skill Condition ==================================
        public int? Combo { get; private set; }         // how many attacks the caster has made
        // ...

        // every parameter is optional so a caller fills in only what it knows 
        public SkillStepContext(IDamageable me = null, IDamageable recipient = null,
                                Vector3? position = null, int? combo = null)
        {
            Me = me;
            Recipient = recipient;
            Position = position;
            Combo = combo;
        }
    }
}
