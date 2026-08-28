using UnityEngine;

namespace MagicSchool.Items
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Magic School 3/Item")]
    public class ItemDataSO : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private string _name = "Item A";
        [SerializeField, TextArea] private string _description = "Does nothing yet.";

        // ===================== setter & getter =====================
        public GameObject Prefab => _prefab;
        public string Name => _name;
        public string Description => _description;
    }
}
