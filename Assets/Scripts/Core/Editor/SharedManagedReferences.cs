using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// A [SerializeReference] field is written to the asset as an id pointing into the shared
    /// "references:" block at the bottom of the file, so duplicating a list element copies the id and
    /// not the object behind it - the new element ends up aliasing the original one. Editing either
    /// then edits both, which is never what duplicating an element is meant to mean here.
    ///
    /// Unity has no "deep-copy the managed references" hook to override, and the aliasing can come in
    /// through the list's + button, Ctrl+D, the right-click Duplicate Array Element menu, or pasting a
    /// whole step. Rather than patch each of those separately, this walks the finished object and
    /// hands every repeated instance its own copy - so however the duplicate was made, it comes out
    /// independent.
    /// </summary>
    public static class SharedManagedReferences
    {
        /// <summary>
        /// Give every managed reference that turns up more than once in this object its own copy.
        /// Returns true if anything actually had to be split.
        /// </summary>
        public static bool Split(SerializedObject serializedObject)
        {
            // reference identity, not Equals: two conditions that happen to hold the same values are
            // fine, one condition sitting in two places is the bug being fixed
            HashSet<object> seen = new HashSet<object>(ReferenceComparer.Instance);
            bool splitAnything = false;

            // Next(true), not NextVisible(true) - a collapsed list hides its elements from the
            // "visible" walk, and an alias hiding inside one still has to be found
            SerializedProperty property = serializedObject.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ManagedReference) continue;

                object value = property.managedReferenceValue;
                if (value == null) continue;

                // first sighting of this instance - it is the one that gets to keep it
                if (seen.Add(value)) continue;

                object copy = Copy(value);
                property.managedReferenceValue = copy;
                seen.Add(copy);
                splitAnything = true;
            }

            // applied (rather than ...WithoutUndo) so the split lands in the same undo step as the
            // duplicate that caused it, and one Ctrl+Z takes back both
            if (splitAnything) serializedObject.ApplyModifiedProperties();

            return splitAnything;
        }

        /// <summary>
        /// A copy that goes all the way down: nested managed references (an effect's own conditions,
        /// say) are copied too, otherwise splitting the effect would leave its conditions still shared.
        /// </summary>
        private static object Copy(object source)
        {
            if (source == null) return null;

            Type type = source.GetType();

            // copies itself
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) return source;

            // an asset reference must keep pointing at the same asset, never at a copy of it
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return source;

            if (type.IsArray)
            {
                Array sourceArray = (Array)source;
                Array copyArray = Array.CreateInstance(type.GetElementType(), sourceArray.Length);
                for (int i = 0; i < sourceArray.Length; i++) copyArray.SetValue(Copy(sourceArray.GetValue(i)), i);
                return copyArray;
            }

            if (source is IList sourceList)
            {
                IList copyList = (IList)Activator.CreateInstance(type);
                foreach (object element in sourceList) copyList.Add(Copy(element));
                return copyList;
            }

            // a plain [Serializable] class or struct. Everything Unity can put behind a managed
            // reference is constructible without arguments, which is what lets this stay generic.
            object copy = Activator.CreateInstance(type, true);
            foreach (FieldInfo field in SerializedFields(type))
            {
                field.SetValue(copy, Copy(field.GetValue(source)));
            }
            return copy;
        }

        /// <summary>
        /// The fields Unity itself would have written out - copying anything else would put values
        /// into the copy that the asset never held. DeclaredOnly plus the walk up BaseType because a
        /// base class's private fields (SkillCondition._subject, SkillEffect._conditions) are
        /// serialized but invisible to a plain GetFields on the concrete type.
        /// </summary>
        private static IEnumerable<FieldInfo> SerializedFields(Type type)
        {
            const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public
                                        | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(Declared))
                {
                    if (field.IsNotSerialized || field.IsInitOnly) continue;

                    bool serialized = field.IsPublic
                                   || field.IsDefined(typeof(SerializeField), true)
                                   || field.IsDefined(typeof(SerializeReference), true);

                    if (serialized) yield return field;
                }
            }
        }

        /// Hand-rolled because System's ReferenceEqualityComparer is .NET 5+, which Unity is not on.
        private class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object a, object b) => ReferenceEquals(a, b);
            public int GetHashCode(object value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }
    }
}
