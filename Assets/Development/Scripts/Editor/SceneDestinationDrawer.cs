using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneDestination))]
public class SceneDestinationDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect sceneRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect spawnIdRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty sceneProp = property.FindPropertyRelative("scene");
        SerializedProperty spawnIdProp = property.FindPropertyRelative("spawnPointId");

        EditorGUI.PropertyField(sceneRect, sceneProp);

        if (sceneProp.objectReferenceValue != null)
        {
            SceneReferenceSO sceneRef = sceneProp.objectReferenceValue as SceneReferenceSO;
            
            if (sceneRef != null && sceneRef.sceneSpawnPoints != null && sceneRef.sceneSpawnPoints.Length > 0)
            {
                string[] options = sceneRef.sceneSpawnPoints;
                
                int currentIndex = Mathf.Max(0, System.Array.IndexOf(options, spawnIdProp.stringValue));
                
                currentIndex = EditorGUI.Popup(spawnIdRect, "Spawn Point ID", currentIndex, options);
                
                spawnIdProp.stringValue = options[currentIndex];
            }
            else
            {
                EditorGUI.PropertyField(spawnIdRect, spawnIdProp);
            }
        }
        else
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(spawnIdRect, spawnIdProp);
            GUI.enabled = true;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight * 2) + 2; 
    }
}
