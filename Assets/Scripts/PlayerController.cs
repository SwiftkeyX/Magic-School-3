using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    private bool _isHeroHolded = false;
    private Hero _heroHolded;


    void Update()
    {
        PlayerMoveHero();
    }

    private void PlayerMoveHero()
    {
        // get world position from current pointer position (mouse)
        Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit == null) return;

        // if hero is not holded by the player, polling until player click a hero
        if (!_isHeroHolded)
        {
            // if the user click/hold on the hero, continue 
            _heroHolded = hit.GetComponent<Hero>();
            bool isHeroHit = (_heroHolded != null);
            bool isHeroClicked = PlayerInputSystem.PointerPressedThisFrame && isHeroHit;
            if (isHeroClicked)
            {
                _isHeroHolded = PlayerInputSystem.IsPointerDown;
            }
        }

        // if hero in holded now, find which hex player will place the hero on
        if (_isHeroHolded)
        {
            // calculate _isHeroHolded for the next frame
            _isHeroHolded = PlayerInputSystem.IsPointerDown;

            // if the user release hero on the hex, place hero on top of the hex 
            Hex targetHex = hit.GetComponent<Hex>();
            bool isRelease = PlayerInputSystem.PointerReleasedThisFrame;
            bool isHexHit = (targetHex != null);
            if (isRelease && isHexHit)
            {
                _heroHolded.MoveHeroInPreparationState(targetHex);
            }
        }
    }
}