using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneryImage))]
public class SceneryImageEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SceneryImage sceneryImage = (SceneryImage)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load image"))
        {
            sceneryImage.LoadImage(sceneryImage.imagePath);
        }
        if (GUILayout.Button("Unload image"))
        {
            sceneryImage.UnloadImage();
        }
        GUILayout.EndHorizontal();
    }

}
