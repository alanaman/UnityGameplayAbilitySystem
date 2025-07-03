using H2V.GameplayAbilitySystem.TagSystem;
using UnityEditor;
using UnityEngine;

namespace H2V.GameplayAbilitySystem.Editor.TagSystem
{
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GameplayTagSet))]
public class GameplayTagSetDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tagsProp = property.FindPropertyRelative("tags");
        SerializedProperty valuesProp = tagsProp.FindPropertyRelative("values");
        EditorGUI.PropertyField(position, valuesProp, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty tagsProp = property.FindPropertyRelative("tags");
        SerializedProperty valuesProp = tagsProp.FindPropertyRelative("values");
        return EditorGUI.GetPropertyHeight(valuesProp, label, true);
    }
}
#endif
}