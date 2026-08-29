using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    // context: a modifier use ModifierEnum to specify which stat is increase to.
    // ModifierTarget is the lookup table for this context.
    // FIXLATER: I don't like the class's name
    internal static class ModifierTarget
    {
        // pair of modifier & stat - tell which stat is increase by this modifier
        private static readonly Dictionary<ModifierEnum, StatEnum> _lookup = new Dictionary<ModifierEnum, StatEnum>
        {
            // buff
            { ModifierEnum.BonusHP        , StatEnum.MAXHP }          ,
            { ModifierEnum.ATK            , StatEnum.ATK }            ,
            { ModifierEnum.AS             , StatEnum.AS}              ,
            { ModifierEnum.DF             , StatEnum.DF }             ,
            { ModifierEnum.DamageReduction, StatEnum.DamageReduction },
            { ModifierEnum.AP             , StatEnum.AP }             ,
            { ModifierEnum.Range          , StatEnum.Range }          ,
            { ModifierEnum.StartMana      , StatEnum.StartMana }      ,
            // ...

            // debuff
            { ModifierEnum.DefendShred    , StatEnum.DF }             ,
            // ...
        };

        // get which stat the modifier enum increase to.
        // ps. return false for status modifier.
        public static bool TryGet(ModifierEnum modifier, out StatEnum stat)
            => _lookup.TryGetValue(modifier, out stat);

        // FIXLATER: I don't like Of()
        // the same, as an optional - for callers that just want to hand it straight to a scaling.
        public static StatEnum? Of(ModifierEnum modifier)
            => _lookup.TryGetValue(modifier, out StatEnum stat) ? stat : null;
    }
}
