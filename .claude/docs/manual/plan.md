# Plan

## Interrupt 


## implement new system 
I want a UI that show how much damage each hero does, how much damage each hero receive, how much damage each hero heal/omnivamp/reduce healing/etc...
I could say I want almost everything that can show number.
- split the AD and AP properly to indicate which does ad or ap damage. => not neccessary since both dmg type was mitigated by def eventually, no merit for splitting them visually. also we already have auto-attack & skill split which I think is enough.
- implement how much shield is give, but shield is not built yet, so skip this one for now.


## implement new hero 
Verity - do direct damage (like auto attack does) 4 time in sequence. choose weakest enemy in 2 hex of current target.
While using skill become untargetable.
weakest enemy = lowest effective health pool
- Implement new template action, "DirectDamage"
- Implement new status, "Untargetable"

Nasus - choose 4 nearest enemy, debuff them, and buff self for that amount, last 8 sec

Solace's 3rd cast

## implemented hero that is unfinished
Garen - can walk while using his skill
- implement can walk while casting *for some skill

Poppy - implement "Recieving Projectile" and Shield
- receiving Projectile sound dump, let skip it for now.

Shen - shield 2 lowest ally. OnExpired, shield burst into AOE, gain second shield.
- implement aim on lowest ally.

Lumen's chakram

Ashe - shoot 8 arrow in a cone at once.
- add to "Fire timing", "shoot at once"

Kaisa - shoot 15 homing projectile in sequence. at 4 nearest enemies

## test
write a MagicSchool.Combat.Tests asmdef.
- not optional anymore if we test FindEnemy: it's internal, so nothing outside Combat can call it.

when there's 2 clustered of enemies, the cluster target is always set to the same target as previous one, never set to the second one. 
The ClusteredCircle still have problem, not sure about other clustered.

## refactor
internal for variable - a ton of work
- skip it for now

Modifer part are some of the most confusing, since the name don't explain itself, we have to look into it later.

## Do last
I don't sure. BUT I think the attack dash animation got override by walk animation. in Vharn scene. 

Could we also make the text in HeroPanel scale as the screen size grow.

The sprite order layer is so random, I want to have a code organize it later.
e.g. order = {hero = 0, item = 1, skill = 2, etc...}

when hero count was over the limit, the player can still start the battle which is not intented.

trip .uxml and .uss comment.

The AOE from the projectile explode when projectile hit somthing. But the AOE should originate from the center of the landed hex.
Not exactly where projectile landed.


## Quick Note
_lookup missing AP and Omnivamp — both read as statuses and silently do nothing.