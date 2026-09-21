# Introduction
This is the manual for the user to write what he think down.
Specifically for designing hero and trait for auto-chess.

Resource:
@trait-design
@hero-deisgn

# 20/9/2569
This is very boring, I can't get to flow state at all.
Let get this done as soon as possible and get to code stage.
If there's something broke, we can just re-design at code stage.

I try look at each trait grid in tft-set9, tft-set10, and tft-set11. And got confused by the cost spread.
I see them spam several 1 cost unit in the same trait but somehow the cost spread look okay?
I don't know how.

I also have to clean up the google-sheet afterward, it really is messy.
- Heroes tab: "Changes from" column is messy
- Rule tab: 

I try letting AI do all 3-5 cost and it not working well. I think I can do this part myself first.
Writiing cost archetype for each origin.

Each Origin should be able to tell the player like me by glancing at its roster. Is it 4flex? 3cost reroll?
The carry should also be distinct to other trait.
What is their carry archetype melee, range, ad, ap, as focus?

Then, after we have a idea how this comp should be play base on what kinda cost archetype it have.

Hey, I actually can get to flow state once I really hurry and try to make thing done. 
I think the secret is to getting thing done early to get that feedback and keep the momentum going by getting done.

# 21/9/69
[x] "Match this minimum spread"
After that we'll have grid table Origin x Class. We'll have a draft version of what each trait will be played base on cost archetype.
Next would be polish the 3-5 cost roster by matching the minimum spread I give (Do only 3-5 cost):
    for carry: 5/4/7/7/3
    for tank: 5/5/3/4/1
* Also add this number to the rule too.

[x] "Make 1-2 cost"
Now we have all the roster for 3-5 cost, next is to make 1-2 cost.
Goal of making those low cost unit is to make sure the SpiderWeb is working. 
After making 1-2 cost, let try make all possible comp using every cost by having a limitation of 8-9 unit.
And let see how it look. I think we should get something useful for creating 1-2 cost. 
* SpiderWeb is all combination of each hero to maximize the active trait.

This is a process of trial and error. So we'll make several version of it.
And decide what is the best one.

[x] "SpiderWeb idea"
The idea is make 1 version, make spiderweb.
make 2nd version, make spiderweb.
and make 3rd version, make spiderweb.
Each time we should get the idea of what's good and bad, we'll try iterate this until I am satisfied.

[] "1-2 cost review"
I'll review each Origin myself one by one. To see if some unit seem off or not.
If we pass the review, we'll get the strong initial roster, the member should stay this way and not be altered.

[] "Polish initial roster"
Next is adding more stub hero until we reach 56 member. This is the stage we will add new stub member just so we can make testing the spiderweb.
If it not working fine, we'll return to initial roster and remake stub heroes again.
* stub heroes is just hero to add the member to 56 and make the strong spiderweb 

[] "Spiderweb Stage"
For the spiderweb, I see you have a problem with it. And also I have no idea what this spiderweb you give me.
Simply, the spiderweb now is unusable. I will give you a framework to follow:
1) Let focus on 1 origin at a time. Let divided the spiderweb into each BP tier, for example:
    At 9 units total.
    Comrade 2: have Resolute 4 => this is one comp.
    Comrade 4: have Resolute 2, Gambit 2, Protector 2 => this is second comp.
2) Point system: The deeper the trait is the more point it give.
    BP tier 1 always give 1 point.
    BP tier 2 always give 3 point.
    BP tier 3 always give 5 point.
For example, Comrade 2 give 1 point but Comrade 4 give 3 point.
3) Each BP tier should contain 3 best comp, which mean the comp with the most point.

...

Cleanup:
[x] the rule tabs is always messy in the end. Let clean it afterward.
    - Could we make the first column state the short practical rule that we can follow. Then, make a second column explain why this rule exist.
[] class is also messy
    - I see some non-sense in class tab, specifically the "Hidden role - why". Is this important? If yes, shorten it. If no, delete it.
[] Let make you clean those Origin, Class another. Then, I'll lock it since they're begin to be more stable and need less attention.


