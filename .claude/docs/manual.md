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
Teemo - for testing circle AOE with DOT - also need to implement homing projectile here
Cassiopeia - for testing homing projectile
Jhin - implementing pierce projectile
Samira - implementing first hit projectile
Teemo - implement new trigger "OnProjectileHit" 
=> we already have OnExpired which work exactly like what I want 
=> But the CircleAOE doesn't consume where projectile expired but it consume enemy position directly which is not intended
=> Implement new aim target = "WhereProjectileHit"
Karma - test first hit projectile - AOE explode on impact

Hey, I forget about AttackSkillEffect. That still doesn't implement stat scaling.

Aatrox (cont.)
=> It would be nice to see the progress bar of the skill's duration - the white progress bar just above healthbar 
=> Also Omnivamp should be included in Stat too, now it does nothing - Omnivap heal self base on damage they did

I don't sure. BUT I think the attack dash animation got override by walk animation. in Aatrox scene. 

Cassiopeia - implement effect modifier +30% dmg amp when target is wounded
Jhin - implement attack falloff per hit
Samira - implement new debuff to her skill - reduce target's armor

=== implement new hero ===
Poppy - implement "Recieving Projectile" and Shield
Sona - use Pierce Projectile but with modified AOE size & buff ally who get hit
- implement modified AOE size
- implement ally as recipient
JarvanIV - jump into the cluster enemies. Stun AOE.
- implement jump
