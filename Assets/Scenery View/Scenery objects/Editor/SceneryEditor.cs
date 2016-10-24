using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Scenery))]
public class SceneryEditor : Editor
{

    string path;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Scenery scenery = (Scenery)target;

        GUILayout.Space(20);

        GUILayout.Label("Saving and loading the scenery externally");

        // Path input field
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Path");
        path = EditorGUILayout.TextField(path);
        GUILayout.EndHorizontal();

        // Save and load buttons
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Save scenery"))
        {
            scenery.SaveScenery(path);
        }
        
        if (GUILayout.Button("Load scenery"))
        {
            scenery.LoadScenery(path);
        }

        GUILayout.EndHorizontal();
    }

}
