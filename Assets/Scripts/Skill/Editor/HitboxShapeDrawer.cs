using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicSchool
{
    // Same gap as SkillEffectDrawer: Unity's stock Inspector has no built-in dropdown for picking a
    // concrete type for a [SerializeReference] field. This fills it for HitboxShape (Box/Circle/Cone).
    [CustomPropertyDrawer(typeof(HitboxShape), true)]
    public class HitboxShapeDrawer : PropertyDrawer
    {
        private static Type[] _concreteTypes;

        private static Type[] ConcreteTypes()
        {
            if (_concreteTypes == null)
            {
                _concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(SafeGetTypes)
                    .Where(t => typeof(HitboxShape).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .OrderBy(t => t.Name)
                    .ToArray();
            }
            return _concreteTypes;
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (System.Reflection.ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Type[] types = ConcreteTypes();
            string[] popupOptions = new string[types.Length + 1];
            popupOptions[0] = "<None>";
            for (int i = 0; i < types.Length; i++) popupOptions[i + 1] = types[i].Name;

            Type currentType = property.managedReferenceValue?.GetType();
            int currentIndex = currentType == null ? 0 : Array.IndexOf(types, currentType) + 1;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, popupOptions);
            if (EditorGUI.EndChangeCheck())
            {
                property.managedReferenceValue = newIndex <= 0 ? null : Activator.CreateInstance(types[newIndex - 1]);
            }

            EditorGUI.EndProperty();
        }
    }
}
