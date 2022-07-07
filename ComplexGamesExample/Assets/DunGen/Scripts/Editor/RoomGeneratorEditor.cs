using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(RoomGenerator))]
public class RoomGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        

        RoomGenerator myScript = (RoomGenerator)target;
        SerializedObject baseObj = new SerializedObject(target);

        baseObj.Update();

        myScript.manulallySelectNodes = GUILayout.Toggle(myScript.manulallySelectNodes, "Mannually Select Nodes");
        if (!myScript.manulallySelectNodes)
        {
            EditorGUILayout.PropertyField(baseObj.FindProperty("extraNodes"));
            EditorGUILayout.PropertyField(baseObj.FindProperty("startRoomFab"));
            EditorGUILayout.PropertyField(baseObj.FindProperty("endRoomFab"));
        }
        if (myScript.manulallySelectNodes)
        {
            EditorGUILayout.PropertyField(baseObj.FindProperty("required"), true);
        }

        EditorGUILayout.PropertyField(baseObj.FindProperty("corriderRooms"));
        EditorGUILayout.PropertyField(baseObj.FindProperty("cornerRooms"));
        EditorGUILayout.PropertyField(baseObj.FindProperty("generateOnRuntime"));
        if (!myScript.GORT && GUILayout.Button("Generate New Dungeon"))
        {
            myScript.MakeDungeon();
            EditorUtility.SetDirty(baseObj.targetObject);
        }

        baseObj.ApplyModifiedProperties();
    }
}
