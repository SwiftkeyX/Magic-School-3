using System;

namespace MagicSchool.Combat.Heroes
{
    /// <summary>
    /// Which hero the player is inspecting. One at a time, null means nothing is selected.
    ///
    /// This lives in Combat because it is the only assembly both sides of the conversation can
    /// see: Player raises the selection (it owns the mouse) and UI draws it (it owns the panel),
    /// and those two do not reference each other - deliberately.
    ///
    /// FLAGGING: static, so it is the same reach-in smell as GameManager.Instance. It is here
    /// because a wired service would need a scene object and two Inspector references to say one
    /// thing. If selection ever grows past "one hero at a time" - multi-select, a comparison
    /// view, selection history - make it a real object and inject it.
    /// </summary>
    /// 
    /// FIXLATER: This is not need to be in Hero module, just move it to UI module.
    public static class HeroSelection
    {
        public static Hero Selected { get; private set; }

        // Raised on every change, including deselection - listeners get null and close themselves.
        public static event Action<Hero> Changed;

        public static void Select(Hero hero)
        {
            // clicking the hero that is already open is not a change, and re-raising would make
            // a listener that rebuilds itself flicker for no reason
            if (ReferenceEquals(Selected, hero)) return;

            Selected = hero;
            Changed?.Invoke(hero);
        }

        public static void Clear() => Select(null);

        /// <summary>
        /// Statics survive a scene load but the heroes they point at do not, so whoever owns the
        /// lifetime of a battle should call this when tearing one down. Without it the panel can
        /// come back up pointing at a destroyed hero.
        /// </summary>
        public static void Reset()
        {
            Selected = null;
            Changed = null;
        }
    }
}
