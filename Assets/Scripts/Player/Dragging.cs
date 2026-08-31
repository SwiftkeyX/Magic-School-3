using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Core;
using MagicSchool.Combat.Heroes;
using MagicSchool.Combat.Placements;
using MagicSchool.Items;

namespace MagicSchool.Player
{
    // Picking things up with the pointer, dragging around, and putting them down again.
    internal class Dragging
    {
        private readonly Camera _cam;
        private readonly BattleBoard _board;
        private readonly ISellZone _sellZone;
        private readonly TeamEnum _team;

        private IDraggable _held;
        private Collider2D _heldHitbox;
        private bool _sellHintShown;

        // ============================= getter =============================
        private int HeroLimit => GameManager.Instance.HeroLimit;
        public bool IsHolding => _held as Object != null;

        public Dragging(Camera cam, BattleBoard board, ISellZone sellZone, TeamEnum team)
        {
            _cam = cam;
            _board = board;
            _sellZone = sellZone;
            _team = team;
        }

        // =================================== life cycle ===================================
        public void Tick()
        {
            // dragging is a preparation-phase thing; a fight starting mid-drag puts it back down
            if (GameManager.Instance.Phase != GamePhaseEnum.Preparation)
            {
                if (IsHolding) Cancel();
                return;
            }

            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);

            // if not holding anything, polling until something was holded
            if (!IsHolding)
            {
                TryPickUp(worldPos);
                return;
            }

            // the holding object follow the pointer, create a visually object dragging
            Follow(worldPos);

            // if the object is sellable, drag it to shop, will turn on the shop hint
            // shop hint = make shop red to indicated it was focus
            bool isSellable = _held is Hero;
            RefreshSellHint(isSellable && IsPointerOverSellZone());

            // if still holding, return
            bool stillHolding = PlayerInputSystem.IsPointerDown && !PlayerInputSystem.DragReleasedThisFrame;
            if (stillHolding) return;

            // if releasing, drop the hero 
            Drop(worldPos);
        }

        // =================================== pick up ===================================
        // the pickup could be either hero, or item.
        private void TryPickUp(Vector3 worldPos)
        {
            if (!PlayerInputSystem.DragPressedThisFrame) return;

            IDraggable hero = TryPickHero(worldPos);
            if (hero != null)
            {
                Grab(hero);
                return;
            }

            IDraggable item = TryPickItem(worldPos);
            if (item != null)
            {
                Grab(item);
                return;
            }
        }

        private IDraggable TryPickHero(Vector3 worldPos)
        {
            Hero hero = Picker.At<Hero>(worldPos);
            if (CanDragThisHero(hero))
            {
                return hero;
            }

            return null;
        }

        private IDraggable TryPickItem(Vector3 worldPos)
        {
            Item item = Picker.At<Item>(worldPos);
            if (item == null) { return null; }

            // if the item have wearer, take it off
            Hero wearer = item.GetComponentInParent<Hero>();
            if (wearer != null)
            {
                wearer.TryTakeOff(item);
            }

            return item;
        }

        // There's some hero that player shouldn't allow to drag
        // e.g. enemy
        private bool CanDragThisHero(Hero hero)
        {
            return hero != null && hero.Team == _team;
        }

        private void Grab(IDraggable target)
        {
            _held = target;
            _heldHitbox = target.transform.GetComponent<Collider2D>();
            if (_heldHitbox != null) _heldHitbox.enabled = false;
        }

        private void Follow(Vector3 worldPos)
        {
            Transform held = _held.transform;
            held.position = new Vector3(worldPos.x, worldPos.y, held.position.z);
        }

        // =================================== drop ===================================
        private void Drop(Vector3 worldPos)
        {
            if (_held is Hero hero)
            {
                DropHero(hero, worldPos);
                return;
            }

            if (_held is Item item)
            {
                DropItem(item, worldPos);
                return;
            }

            Release();
        }

        // hero could be drop on 2 thing:
        // 1) placement => put that hero on a placement
        // 2) shop => sell that hero 
        private void DropHero(Hero hero, Vector3 worldPos)
        {
            IPlacement targetPlacement = Picker.At<IPlacement>(worldPos);

            // released on a placement = put a hero there
            if (targetPlacement != null && ValidatePlacement(targetPlacement, hero))
            {
                GameManager.Instance.MoveHero(hero, targetPlacement);
                Release();
                return;
            }

            // released over the shop = sell a hero.
            if (targetPlacement == null && IsPointerOverSellZone())
            {
                GameManager.Instance.SellHero(hero);
                Release();
                return;
            }

            // released on nothing = put hero back to its original placement
            Cancel();
        }

        // an item can be dropped on 1 thing so far:
        // 1) one of your heroes => that hero wears it, if it has a slot free
        private void DropItem(Item item, Vector3 worldPos)
        {
            Hero hero = Picker.At<Hero>(worldPos);

            Release();

            // wear the item to your hero
            if (hero == null || hero.Team != _team) return;
            hero.TryWear(item);
        }

        // put back whatever is held, as far as it can be put back
        private void Cancel()
        {
            // only a hero has somewhere to be put back to; an item stays where it is
            if (_held is Hero hero)
            {
                IPlacement placement = hero.CurrentPlacement;

                // OnUnitPlaced re-seats the transform, so this is the snap back
                if (placement != null) placement.OnUnitPlaced(hero);
            }

            Release();
        }

        // when stop holding, reset var
        private void Release()
        {
            if (_heldHitbox != null) _heldHitbox.enabled = true;

            // nothing is held any more, so the shop must stop offering to take it
            RefreshSellHint(false);

            _heldHitbox = null;
            _held = null;
        }

        // check the placement before placing the hero
        private bool ValidatePlacement(IPlacement placement, Hero hero)
        {
            bool validate = false;

            // the placement is already taken by someone, not allow the placement
            if (IsPlacementTaken(placement, hero)) return false;

            // if placement is bench, allow the placement
            if (placement is BenchSlot)
            {
                validate = true;
            }

            // if placement is hex, check the which team hex belong to, and current hero limit
            // if correct, allow the placement
            // p.s. Hex could only belong to either Red or Blue team
            else if (placement is Hex targetHex)
            {
                bool correctTeam = targetHex.GetTeam() == _team;

                // if the hero placing here is the new hero that arn't already on the board,
                // check if new hero count is over the allow hero limit.
                bool isThisHeroTracked = _board.HeroesOnBoard.Any(tracked => (Hero)tracked == hero);
                int myHeroCount = _board.CountTeamOnBoard(_team);
                if (!isThisHeroTracked) myHeroCount += 1;
                bool isHeroesCountOverFlow = myHeroCount > HeroLimit;

                validate = correctTeam && !isHeroesCountOverFlow;
            }

            return validate;
        }

        // Is somebody already standing here? 
        private bool IsPlacementTaken(IPlacement placement, Hero hero)
        {
            if (placement is Hex hex) return _board.IsReservedByOther(hex, hero);

            if (placement is BenchSlot slot) return slot.Reserved;

            return false;
        }

        // =================================== sell zone ===================================
        // is player's pointer over shop boundary?
        private bool IsPointerOverSellZone()
        {
            return _sellZone != null && _sellZone.IsInSellBoundary(PlayerInputSystem.PointerScreenPosition);
        }

        private void RefreshSellHint(bool isOverZone)
        {
            if (isOverZone == _sellHintShown) return;

            _sellZone?.ShowSellHint(isOverZone);
            _sellHintShown = isOverZone;
        }
    }
}
