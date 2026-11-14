# Trait Design

## Note




## How do we actually design trait
Trait are separate into several template according serveral auto-chess having kinda similar trait.
So we'll just fill each one and build from the foundation

### Trait archetype
#### Neccessary layer
[x] Stat trait
Trait that give you stat.

[x] Stall trait
Trait that get stronger but late in the fight
e.g. set 9 shurima

[x] Item trait
Trait that give you item.

[x] Summon trait
Every auto-chess have a trait than can summon something to help.
e.g. set 10 Country

[x] Damage board trait 
auto-chess usually have the trait that for a certain amount of time pass, the entire board will get damage by something
e.g. set 9 Freijord, set 12 Dragonlord

[x] Scaling trait 
A trait need a certain amount of round pass so they can scale, if they don't get this scale condition, they fell off late game.
e.g. set 9 Noxus, set 11 Mystic

[x] Status trait
e.g. set 9 Freijord

[x] Horizontal trait for Tank/Carry
There's a template for horizontal trait that was used almost every set in TFT
e.g. Warden, Bruiser, Gunner, Vanquisher, Slayer, Invoker, Mage, etc...

[] Utility trait
A class that also increase your stat but easier to use, those unit are common enough to fit it on your team.
e.g. Heavenly = each heavenly give different stat, Dazzler = make enemy deal less damage, Strategist, Superfan, 

#### Additional layer
[] Hex trait
A trait that need you to stand on specific hex.
e.g. set 10 KDA, set 11 Umbral

[] Lasercorp clone trait
A cool trait that don't look like anything, but it look hard to balance though.

[] Random trait
TFT and Tales&Tactic both have random trait that are randomly assign to hero.
e.g. set 11 Exalted 

[] Threat clone trait
TFT have this trait that aren't actually is trait.
This is cool, but it wasn't needed.

### Cost Archetype
1. Cost archetype decide how the trait was played:
    I notice that each trait from TFT have a different late game carry.
        - It's either 3-cost, 4-cost, or 5-cost carry. 
        - Sometimes they have both 4-cost carry, 4-cost tank. 
        - Sometimes they have both 5-cost carry, 4-cost tank. 
        - Sometimes they have duo 4-cost carry, and 3-cost tank.
        - Sometimes they carry is 3-cost, tank is 4-cost 
        - etc...
    Those create how each trait is played. For example, Set 11 Storyweaver only have 1 reliable lategame 5-cost carry.
    So they need to get to lvl9 and healthy enought to roll for the carry. 

2. Cost archetype breakdown - To create identity to each trait, we want to have all this cost archetype. 

    Context: Because of TFT mechanic, player can usually go for 1 main tank, 1-2 main carry.

    4-cost flex - This is the type of team that use 4-cost as their main tank and carry
        Standard - 4-cost 1 main tank and 4-cost 1 main carry e.g. Demacia
        Duo carry - 4-cost 2 main carry. And find random tank else where e.g. Ghostly

    4-5-cost flex - This is the type of team that have their key unit in 5-cost.
        Standard - 4-cost main tank and 5-cost main carry e.g. Storyweaver
        Duo carry - 4-cost main carry and 5-cost secondary carry e.g. Fated

    3-cost reroll - This is the type of team that use 3-cost as their main tank and carry
        Standary - 3-cost main tank and carry e.g. Mystic.
        Duo carry - 3-cost 2 main carry. And 2-cost main tank e.g. Noxus

### Cost Spread
I try look at each trait grid in tft-set9, tft-set10, and tft-set11. And got confused by the cost spread.
I see them spam several 1 cost unit in the same trait but somehow the cost spread look okay? I don't know how.