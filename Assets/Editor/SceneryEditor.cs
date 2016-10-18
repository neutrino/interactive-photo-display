using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Scenery))]
public class SceneryEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Scenery scenery = (Scenery)target;

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load all scenery images"))
        {
            foreach (SceneryImage SI in scenery.SceneryImages())
            {
                SI.LoadImage(SI.imagePath);
            }
        }
        if (GUILayout.Button("Unload all scenery images"))
        {
            foreach (SceneryImage sceneryImage in scenery.SceneryImages())
            {
                sceneryImage.UnloadImage();
            }
        }
        GUILayout.EndHorizontal();
    }

}
