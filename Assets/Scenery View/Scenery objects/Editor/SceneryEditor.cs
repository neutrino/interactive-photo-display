using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Scenery))]
public class SceneryEditor : Editor
{

    string sourcePath;
    string targetPath;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Scenery scenery = (Scenery)target;

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        targetPath = EditorGUILayout.TextField(targetPath);
        if (GUILayout.Button("Save scenery"))
        {
            scenery.SaveScenery(targetPath);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        sourcePath = EditorGUILayout.TextField(sourcePath);
        if (GUILayout.Button("Load scenery"))
        {
            scenery.LoadScenery(sourcePath);
        }
        GUILayout.EndHorizontal();
    }

}
