using UnityEditor;

namespace MagicSchool
{
    /// <summary>
    /// Draws exactly the stock Inspector - it exists only to run the managed-reference split, because
    /// every [SerializeReference] in the project (an action group's conditions and effects, an
    /// effect's own conditions) hangs off a SkillSO. Duplicating a step or an action group otherwise
    /// leaves the copy sharing the original's conditions, so editing one edits both.
    /// </summary>
    [CustomEditor(typeof(SkillSO))]
    public class SkillSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // after drawing, so it sees the duplicate the + button just made. Also repairs assets
            // that were already saved sharing a reference, the first time they are looked at.
            if (SharedManagedReferences.Split(serializedObject))
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
