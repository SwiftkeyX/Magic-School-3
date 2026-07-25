using UnityEngine;

public class BenchSlot : MonoBehaviour
{
    private bool _reserved = false;

    // ==================== getter ===================
    public bool Reserved => _reserved;

    // ==================== setter ===================
    public void SetReserved(bool value) => _reserved = value;
}