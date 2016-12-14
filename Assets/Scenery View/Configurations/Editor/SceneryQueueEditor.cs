using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SceneryQueue))]
public class SceneryQueueEditor : Editor
{

    string configurationsPath = "";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Configurations path");
        configurationsPath = EditorGUILayout.TextField(configurationsPath);
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Load"))
        {
            SceneryQueue sceneryQueue = (SceneryQueue)target;
            Configurations configs = Configurations.Load(configurationsPath);
            if (configs != null)
            {
                Configurations.SetInstance(configs);
                sceneryQueue.BeginQueue();
            }
        }
        EditorGUI.EndDisabledGroup();
    }
}
