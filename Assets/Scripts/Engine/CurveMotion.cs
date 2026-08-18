using UnityEngine;

namespace MagicSchool.Engine
{
    /// <summary>
    /// One trip from A to B, paced by an AnimationCurve.
    ///
    /// HeroWalk uses it to step one hex, the Move template action to jump across several. Both
    /// wanted the same elapsed/Evaluate/Lerp bookkeeping, and neither module can see the other,
    /// so it lives down here where both can reach it.
    ///
    /// A struct because it is per-trip data with no identity: start a new trip, make a new one.
    /// Tick() writes to _elapsed, so hold it in a plain field - a readonly one can't advance.
    /// </summary>
    public struct CurveMotion
    {
        private readonly Vector3 _start;
        private readonly Vector3 _end;
        private readonly float _duration;
        private readonly AnimationCurve _curve;     // maps elapsed time (0-1) to progress along the path
        private float _elapsed;

        public CurveMotion(Vector3 start, Vector3 end, float duration, AnimationCurve curve)
        {
            _start = start;
            _end = end;
            _duration = Mathf.Max(duration, Mathf.Epsilon);     // a zero duration would divide by zero and never land
            _curve = curve;
            _elapsed = 0f;
        }

        public bool IsFinished => _elapsed >= _duration;

        public Vector3 End => _end;

        // Advance by deltaTime and hand back where the mover should now stand.
        // The last frame returns _end exactly rather than wherever the curve happens to evaluate
        // at t = 1 - a hand-tuned curve doesn't have to end on 1, but the trip still has to.
        public Vector3 Tick(float deltaTime)
        {
            _elapsed += deltaTime;

            if (IsFinished) return _end;

            float t = _elapsed / _duration;
            float progress = _curve != null ? _curve.Evaluate(t) : t;

            return Vector3.Lerp(_start, _end, progress);
        }
    }
}
