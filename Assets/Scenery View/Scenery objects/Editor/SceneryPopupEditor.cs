using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SceneryPopUp))]
public class SceneryPopupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneryPopUp sceneryPopup = (SceneryPopUp)target;

        if (GUILayout.Button("Toggle visibility"))
        {
            sceneryPopup.ToggleVisibility();
        }
    }
}
