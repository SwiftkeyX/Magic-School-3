using UnityEngine;
using UnityEngine.UI;
using MagicSchool.Heroes;

namespace MagicSchool.UI
{
    // World-space mana bar - same setup and pattern as Healthbar, just reading mana instead of HP.
    // ExecuteAlways lets the offset be tweaked live in edit mode / Prefab Mode, without entering Play mode.
    [ExecuteAlways]
    public class Manabar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.8f, 0f);

        private Hero _hero;

        void Awake()
        {
            _hero = GetComponentInParent<Hero>();
        }

        void LateUpdate()
        {
            if (_hero == null) _hero = GetComponentInParent<Hero>();
            transform.position = _hero.transform.position + _offset;

            if (_hero.IsInitialized)
                _slider.value = (float)_hero.CurrentMana / _hero.MaxMana;
        }
    }
}
