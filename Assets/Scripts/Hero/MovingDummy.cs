using UnityEngine;

namespace MagicSchool
{
    /// FIXLATER: This is temp
    /// <summary>
    /// A dummy that slides up and down its column instead of standing still 
    /// </summary>
    public class MovingDummy : MonoBehaviour
    {
        [SerializeField] private float _speed = 2f;        // world units per second
        [SerializeField] private float _distance = 3f;     // how far it travels each way from its hex

        private Hero _me;
        private Vector3 _origin;
        private bool _hasOrigin;

        void Awake()
        {
            _me = GetComponent<Hero>();
        }

        void Update()
        {
            // stand still until the battle is on, same as a hero
            if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;

            if (!TryTakeOrigin()) return;

            // PingPong runs 0..2*distance, so shift it to swing evenly either side of the hex
            float offset = Mathf.PingPong(Time.time * _speed, _distance * 2f) - _distance;
            transform.position = _origin + Vector3.up * offset;
        }

        // The hex is the anchor, not wherever the transform happens to be when this first runs -
        // the hero is placed on its hex a frame or so after being spawned.
        private bool TryTakeOrigin()
        {
            if (_hasOrigin) return true;

            if (_me != null)
            {
                if (!_me.IsInitialized || _me.CurrentHex == null) return false;
                _origin = _me.CurrentHex.transform.position;
            }
            else
            {
                _origin = transform.position;
            }

            _hasOrigin = true;
            return true;
        }
    }
}
