using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicSchool
{
    // Same gap as SkillEffectDrawer: Unity's stock Inspector cannot pick a concrete type for a
    // [SerializeReference] field. Without this a condition can only ever be <None>, so an action
    // group or an effect could never actually be given one.
    [CustomPropertyDrawer(typeof(SkillCondition), true)]
    public class SkillConditionDrawer : PropertyDrawer
    {
        private static Type[] _concreteTypes;

        private static Type[] ConcreteTypes()
        {
            if (_concreteTypes == null)
            {
                _concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(SafeGetTypes)
                    .Where(t => typeof(SkillCondition).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
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

        // conditions sit even deeper than effects, so the label floor matters more here
        private const float MinLabelWidth = 110f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(MinLabelWidth, EditorGUIUtility.labelWidth);

            try
            {
                Type[] types = ConcreteTypes();
                string[] popupOptions = new string[types.Length + 1];
                popupOptions[0] = "<None>";
                for (int i = 0; i < types.Length; i++) popupOptions[i + 1] = types[i].Name;

                Type currentType = property.managedReferenceValue?.GetType();
                int currentIndex = currentType == null ? 0 : Array.IndexOf(types, currentType) + 1;

                Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, popupOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    property.managedReferenceValue = newIndex <= 0 ? null : Activator.CreateInstance(types[newIndex - 1]);
                }

                if (property.managedReferenceValue != null)
                {
                    EditorGUI.indentLevel++;
                    float y = dropdownRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                    SerializedProperty endProperty = property.GetEndProperty();
                    SerializedProperty childProp = property.Copy();
                    bool enterChildren = true;
                    while (childProp.NextVisible(enterChildren) && !SerializedProperty.EqualContents(childProp, endProperty))
                    {
                        enterChildren = false;
                        float h = EditorGUI.GetPropertyHeight(childProp, true);
                        Rect r = new Rect(position.x, y, position.width, h);
                        EditorGUI.PropertyField(r, childProp, true);
                        y += h + EditorGUIUtility.standardVerticalSpacing;
                    }

                    EditorGUI.indentLevel--;
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = previousLabelWidth;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue != null)
            {
                height += EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty childProp = property.Copy();
                bool enterChildren = true;
                while (childProp.NextVisible(enterChildren) && !SerializedProperty.EqualContents(childProp, endProperty))
                {
                    enterChildren = false;
                    height += EditorGUI.GetPropertyHeight(childProp, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }
    }
}
