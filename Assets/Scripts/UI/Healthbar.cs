using UnityEngine;
using UnityEngine.UI;

// World-space health bar
// ExecuteAlways lets the offset be tweaked live in edit mode / Prefab Mode, without entering Play mode.
[ExecuteAlways]
public class Healthbar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, 0f);
    private Image _image;
    private Hero _hero;

    void Awake()
    {
        _hero = GetComponentInParent<Hero>();
        _image = _slider.fillRect.GetComponent<Image>();
    }

    void Start()
    {
        if (_hero.Team == Team.Blue) _image.color = Color.green;
        
        else if (_hero.Team == Team.Red) _image.color = Color.red;
    }

    void LateUpdate()
    {
        if (_hero == null) _hero = GetComponentInParent<Hero>();
        transform.position = _hero.transform.position + _offset;

        // CurrentHP/MaxHP only exist once Hero.Init() has run - skip otherwise. Checking
        // IsInitialized instead of Application.isPlaying because a Hero open in Prefab Mode
        // never gets Init() called even while Play mode is running elsewhere in the editor.
        if (_hero.IsInitialized)
            _slider.value = (float)_hero.GetCurrentHP() / _hero.GetMaxHP();
    }
}
