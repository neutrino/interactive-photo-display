using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Kinect = Windows.Kinect;

[CustomEditor(typeof(BodyTracker))]
public class BodyTrackerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        BodyTracker bodyTracker = (BodyTracker)target;
        
        var trackedIds = bodyTracker.TrackedBodies();
        if (trackedIds != null)
        {
            GUILayout.Label("Tracked bodies: " + trackedIds.Count);
            foreach (ulong id in trackedIds.Keys)
            {
                GUILayout.Label("" + id);
            }
        }

        EditorUtility.SetDirty(target);
    }

}
