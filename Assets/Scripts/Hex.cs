using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    private BattleBoard _board;
    private List<Hex> _neighbors;       // neighbors hex of this hex - to give the hex available for pathfinding logic

    // ========================== getter & setter ==========================
    public string Name => gameObject.name;

    // Hex is a "worker" for BattleBoard. So Hex need reference to him.
    public void SetBoard(BattleBoard board)
    {
        _board = board;
        _neighbors = null;
    }

    // find neighbors hex - the hex that is adjacent to the current hex
    public List<Hex> GetNeighbors()
    {
        // return neighbors if exist
        if (_neighbors != null)
            return _neighbors;

        var hexs = _board.Hexs.Values;

        // find distance between current hex and every hex
        float nearestDist = float.MaxValue;
        foreach (var hex in hexs)
        {
            if (hex == this) continue;
            float dist = Vector3.Distance(transform.position, hex.transform.position);
            if (dist < nearestDist)
                nearestDist = dist;
        }

        // create neighbors of hex as a list
        float threshold = nearestDist * 1.15f;
        _neighbors = new List<Hex>();
        foreach (var hex in hexs)
        {
            if (hex == this) continue;
            if (Vector3.Distance(transform.position, hex.transform.position) <= threshold)
                _neighbors.Add(hex);
        }

        // lastly, return neighbors
        return _neighbors;
    }
}
