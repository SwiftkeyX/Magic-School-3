using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    private BattleBoard _board;
    private List<Hex> _neighbors;       // neighbors hex of this hex - to give the hex available for pathfinding logic

    // ========================== getter & setter ==========================
    public string Name => gameObject.name;
    public List<Hex> Neighbors => _neighbors;
    public Hero Occupant { get; private set; }
    public bool IsOccupied => Occupant != null;

    #region Life Cycle
    void Start()
    {
        InitializeNeighbors();
    }

    #endregion

    #region Setup
    // Hex is a "worker" for BattleBoard. So Hex need reference to BattleBoard.
    public void SetBoard(BattleBoard board)
    {
        _board = board;
        _neighbors = null;
    }
    #endregion

    /// <summary>
    /// Neighbors are the hex adjacent to current hex.
    /// </summary>
    #region Neighbors
    // called by Hero - so hero know which hex is valid to move
    public List<Hex> GetNeighbors()
    {
        if (_neighbors != null) InitializeNeighbors();

        return _neighbors;
    }

    // Neighbors don't create itself, we need to calculate it ourself
    private void InitializeNeighbors()
    {
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
    }
    #endregion

    /// <summary>
    /// Occupant used to tell which hex is already occupied by the hero.
    /// Since 1 hex can have only 1 hero stand on it, we need a variable to 
    /// state which hex is already occupied by a hero to prevent 2 hero standing on the same hex.
    /// </summary>
    #region Occupant
    // A hero standing on / walking toward this hex, reserves the hex for that hero.
    // So only one hero can occupy a hex at a time.
    public void SetOccupant(Hero hero)
    {
        Occupant = hero;
    }

    public void ClearOccupant()
    {
        Occupant = null;
    }
    #endregion
}
