using System.Collections;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f; // hexes per second
    [SerializeField] private Hex _currentHex;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Coroutine _walkRoutine;

    public Hex CurrentHex => _currentHex;

    // Init when hero spawn, because he need to teleport to his hex
    public void Init(Hex startingHex)
    {
        _currentHex = startingHex;
        transform.position = startingHex.transform.position;
    }

    void Start()
    {
        if (_currentHex != null)
            transform.position = _currentHex.transform.position;
    }

    void Update()
    {
        Debug.Log("currentHex: " + _currentHex);
        MoveToAdjacentHex();
    }

    // Walks to a hex adjacent to the one the hero currently stands on.
    private void MoveToAdjacentHex()
    {
        if (_walkRoutine != null) return;

        // Pathfinding to the nearest enemy
        Hex targetHex = _currentHex.GetNeighbors()[0];

        _walkRoutine = StartCoroutine(Walk(targetHex));
    }

    private void PathFinding()
    {
        
    }

    private IEnumerator Walk(Hex targetHex)
    {
        Vector3 start = transform.position;
        Vector3 end = targetHex.transform.position;
        float duration = 1f / _moveSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float easedT = _walkCurve.Evaluate(t / duration);
            transform.position = Vector3.Lerp(start, end, easedT);
            yield return null;
        }

        transform.position = end;
        _currentHex = targetHex;
        _walkRoutine = null;
    }
}
