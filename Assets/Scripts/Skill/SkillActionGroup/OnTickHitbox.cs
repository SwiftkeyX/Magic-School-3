using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// To Apply effect on tick e.g. Anivia's R.
    /// Hitbox will trigger every fix interval (cadence interval), even hero walk into hitbox won't get damage, if the hitbox doesn't trigger.
    /// e.g. you walk into spike but you are fine, until the spike trigger itself
    /// </summary>
    public class OnTickHitbox : Hitbox
    {
        private Hero _caster;
        private readonly List<Hero> _heroesWhoWasHit = new List<Hero>();

        public event Action<Hero> OnHit;

        public void Init(Hero caster)
        {
            _caster = caster;
        }

        // Get all the hero who was hit by the skill
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (_caster == null) return;

            Hero heroHit = other.GetComponent<Hero>();

            // not apply effect to myself, my team, the dead hero
            if (heroHit == null || heroHit.Team == _caster.Team || heroHit.StateType == HeroStateEnum.Dead) return;

            // Group all the heroes who was hit by the skill in 1 list
            if (!_heroesWhoWasHit.Contains(heroHit)) _heroesWhoWasHit.Add(heroHit);
        }

        // Remove hero who was not hit by the skill out of the list
        public void OnTriggerExit2D(Collider2D other)
        {
            Hero hero = other.GetComponent<Hero>();
            if (hero != null) _heroesWhoWasHit.Remove(hero);
        }

        // Called by the owner on its cadence interval - fires OnHit for everyone currently inside
        public void FireTick()
        {
            _heroesWhoWasHit.RemoveAll(hero => hero == null || hero.StateType == HeroStateEnum.Dead);

            foreach (Hero hero in _heroesWhoWasHit)
            {
                OnHit?.Invoke(hero);
            }
        }
    }
}
