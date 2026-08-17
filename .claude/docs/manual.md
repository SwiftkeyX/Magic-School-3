# Skill System Explain
Due to how confusing skill are, I have to make a manual to make it easier to explain:

What is this skill system for? 
To create a modular skill system that can be reuse to construct several big skill.

The entire hero's skill separate into 4 big axes:
1) Step - It is how "smaller skill" in 1 hero skill was order, DUE to hero skill being construct of several smaller skill, 
we need a way to monitor the progression of hero skill. For example:
- Step 1 = OnCast, dunk on enemy
- Step 2 = OnKilled, dunk on new enemy
Suggest reading SkillStep.cs.

2) Trigger - the condition which need to be true to active the said skill. For example:
- OnCast = when hero mana is full, he activate his skill once
- OnKilled = when this hero is the one execute another hero
Suggest reading TriggerType.cs.

3) Action - The said skill that activate. For example:
- dunk on enemy
- shoot projectile
- shoot laser

4) Effect - the effect that was apply to the recipient NOT the target.
4.1) Recipient - the hero who get affected by the skill
4.2) Target - the target hero who this skill was aiming at, BUT target is not necessarily have to be the recipient
For exmaple:
- Darius(user) dunk(action) aim on Enemy(target), BUT ally(recipient) get heal(effect).
- Yasuo(user) slash(action) aim on Enemy(target), the target(recipient) get damaged(effect), BUT yasuo(recipient) get buff(effect).
- Sona(user) shoot wave(action) aim on Furthest enemy(target), BUT all the enemy that got hit by the wave(recipient) get damaged(effect).
Suggest reading EffectRecipientType.cs

This is the example when we combine all of the axes:
Step 1 = OnCast(trigger), Yasuo(user) slash(action) aim on Enemy(target), the same target(recipient) get damaged(effect) and shred(effect).
Step 2 = OnExpired(trigger), Yasuo(user) buff(action) aim on Yasuo(target), the same target(recipient) get buff(effect).

# problem

3) Another thing that I leave untouched is the "Cadence" always being a field to edit, Most of the time Cadence are pair with action.
e.g. ZoneAOE always ahve Cadence true, CircleAOE can have both Cadence true or false
But now even ZoneAOE that always is Cadence true, it now a editable Cadence, if user happen to make the cadence false, the ZoneAOE just stop working since it's guarded that way.
This is maybe confusing if I forget, so I need to leave some NOTE here.
This is intentional for the early stage of implementing skill system, BUT once we get a hang of it, this should be get rid of. 


# Resolve problem
1) "Legacy action = ZoneAOE" and "Recipient = Enemies in Area" are telling the same thing that we want to spawn AOE. 
Maybe we should get rid of one of them? 
OH, okay it actually not the same. This was really complicated. For example:
ZoneAOE could also hit ally IF "Recipient = Ally in Area" instead.
There is more case of this that convince me Recipient is different and was absolutely needed here.


# Resolve idea
Galio's cast have anticipation of his skill expanding and once the duration expired, deal damage.
We don't do that antticipation, BC if we do so, there's going to be a lot of hero that also need cast animation too.
So just skil cast animation for now.


# Plan
Aatrox (cont.)
=> Also Omnivamp should be included in Stat too, now it does nothing - Omnivap heal self base on damage they did

I don't sure. BUT I think the attack dash animation got override by walk animation. in Aatrox scene. 


=== implement new hero ===
Clustered (aimTarget)
- when there's 2 clustered of enemies, the cluster target is always set to the same target as previous one, never set to the second one. 
Which is not intented.
- when there's no clustered, it should aim the current target instead.
- when there's a tie clustered, it should prioritize the current target (if current target was 1 of the candidate)


JarvanIV - jump into the cluster enemies. Stun AOE.
- implement jump

Shen - shield 2 lowest ally. OnExpired, shield burst into AOE, gain second shield.
- implement aim on lowest ally.

Akshan - shoot 6 projectile at the furthest enemy. Shoot in sequence.
- implement new variable in template action, "Fire timing", "shoot in sequence"

Ashe - shoot 8 arrow in a cone at once.
- add to "Fire timing", "shoot at once"

Gwen - do damage by spawn several cone AOE in sequence
- implement AOE with "Fire timing"

Kaisa - shoot 15 homing projectile in sequence.

Jinx - shoot 5 homing projectile in sequince at random enemy in 2 hex of current target.
- Implement new aim target, random (2 hex of current target)

Fiora - do direct damage (like auto attack does) 4 time in sequence. random enemy in 2 hex of current target.
While using skill become untargetable.
- Implement new template action. "DirectDamage"
- Implement new status, "Untargetable"


# Not implement yet
1. Aatrox's AS→AD conversion — reads total AS in attacks/sec, giving 0.56 AD; needs GetBaseStat on IHeroStats and a decision about what "bonus AS" means numerically.
2. ScalingEnum.Flat still inert in GetStatModifier.

Poppy - implement "Recieving Projectile" and Shield
- receiving Projectile sound dump, let skip it for now.

# Note
2. _lookup missing AP and Omnivamp — both read as statuses and silently do nothing.

3. Your original FIXLATER is still true, and swapping the ratio won't fix it. ModifierSkillEffect calls recipient.AddModifier(_modifier, AmplifierFor(recipient), _caster as IHeroStats) — the ratio resolves against the caster's stats. So (StatEnum.AttackSpeed, 25f) would give every ally +25% of Sona's 0.7 = +0.175 flat, not 25% of their own attack speed. Getting "+25% of the ally's AS" needs a modifier that resolves against the recipient, which nothing currently supports.