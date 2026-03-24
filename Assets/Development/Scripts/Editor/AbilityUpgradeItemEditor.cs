using UnityEditor;

[CustomEditor(typeof(AbilityUpgradeItem))]
public class AbilityUpgradeItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "playerAbility", "droneAbility");

        SerializedProperty typeProp = serializedObject.FindProperty("type");

        if (typeProp.enumValueIndex == (int)AbilityUpgradeItem.AbilityType.Player)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerAbility"));
        }
        else if (typeProp.enumValueIndex == (int)AbilityUpgradeItem.AbilityType.Drone)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("droneAbility"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}