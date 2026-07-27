# Review comments — `.tours/review.tour`

Raw comments from the CodeTour review pass over the modular skill system, in tour order.
Some are direct questions, some are self-notes ("come back later" / "test later").
Line numbers are a snapshot at review time - use file + quoted content to relocate after refactors move things.

Check items off as they're addressed.

- [x] **`Assets/Scripts/Board/BattleBoard.cs:69`**
   Find logic enemy now was in 2 place:
   1. HeroStateMachineBlackBoard
   2. BattleBoard

   Find logic enemy would need its own class. So we can get rid of this inconsistency.

- [ ] **`Assets/Scripts/Hero/Hero.cs:67`**
   I still don't sure what skillRuntime does?
   I think it kinda smelled for no reason?

   I'll come back later.

- [x] **`Assets/Scripts/Hero/HeroDataRuntime.cs:37`**
   I don't think Stun or Wound should stay in Stat, no?
   From what I make sense, stat should only contain stuff like ad, as, df, mana, etc...

   But it could work, I'll come back later.

- [x] **`Assets/Scripts/Hero/HeroDataRuntime.cs:48`**
   Could we make DamageReductionPercent become another modifier?
   Since modifier represent buff right?

- [x] **`Assets/Scripts/Hero/HeroDataRuntime.cs:132`**
   Stat's SRP = contain sta data for the hero and possibly getter and setter for those stat BUT not logic for calculating thing e.g. Heal().

   We could have setter for increaseHP BUT not Heal() which calculate is target is wound or not.

   Is that make sense?

- [x] **`Assets/Scripts/Hero/HeroDataRuntime.cs:138`**
   Same to Heal().
   We have a class for modifier already, I think we should move all method that calculate logic for modifier inside it.
   e.g. TickModifiers(), ModifierSum(), HasModifier().

   Then lastly, make Stat call those method like a manager would do. (manager => I don't know the logic BUT I know who I should called)

   Is that make sense?

- [x] **`Assets/Scripts/Hero/HeroStateMachineBlackBoard.cs:80`**
   This seem weird to me.
   Time to make a class for calculate damage espectially, so this TakeDamage() don't have to calculate damage itself.

   Is that make sense too?

- [ ] **`Assets/Scripts/Hero/States/HeroAttack.cs:59`**
   It was fine. BUT I don't like _me.SkillRunTime.().

   StateMachine should only call blackboard only since I add blackboard new's SRP.

   We'll do this later, my idea is to let each state hold ref to blackboard instead of hero.

- [ ] **`Assets/Scripts/Hero/States/HeroStunned.cs:15`**
   I want to test this out.

- [ ] **`Assets/Scripts/Skill/ActionModel.cs:1`**
    I will test this later.

- [ ] **`Assets/Scripts/Skill/ActiveEffect.cs:1`**
    test later.

- [ ] **`Assets/Scripts/Skill/SkillDefinitionSO.cs:1`**
    test later.

- [ ] **`Assets/Scripts/Skill/SkillRuntime.cs:1`**
    test later.

- [ ] **`Assets/Scripts/Hero/Hero.cs:94`**
    I will wire this correctly later.

    e.g. wire using the inspector, not wire here.

- [ ] **`Assets/Scripts/Hero/States/HeroAttack.cs:62`**
    Okay before everything becoming more messy.
    Let dump all data we don't know in _me.BlackBoard.Temp.Method();

    Is that make sense? So we don't make other part beccome too messy.
