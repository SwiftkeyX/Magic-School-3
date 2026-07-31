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
1) "Legacy action = ZoneAOE" and "Recipient = Enemies in Area" are telling the same thing that we want to spawn AOE. 
Maybe we should get rid of one of them? 
OH, okay it actually not the same. This was really complicated. For example:
ZoneAOE could also hit ally IF "Recipient = Ally in Area" instead.
There is more case of this that convince me Recipient is different and was absolutely needed here.

2) Problem with AOE class.
// Maybe I have to change thing up? I think it more suitable to separate 1 AOE into AOE, Point&Click, HitScan, etc... 
// Not everything should be call AOE since it was misleading.

// No, that would change the direction too much, it was too much coupling here.
I think it maybe just naming convention problem here? in google sheet, it was actually good.
Maybe changing the field to "HitboxSize" ?
Yes, I think that better, since it don't misleading the reader like AOE would.

3) Galio still doesn't finished since his skill still doesn't heal over time.

# Plan
Teemo - for testing circle AOE with DOT - also need to implement homing projectile here
Casseoipia - for testing homing projectile
Jhin - implementing pierce projectile
Samira - implementing first hit projectile
Karma - test first hit projectile - AOE explode on impact
