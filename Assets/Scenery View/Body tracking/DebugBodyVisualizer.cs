using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

/*
This component can be used to help body tracking debugging
by drawing simple graphic representations of bodies tracked by BodyTracker.
*/

public class DebugBodyVisualizer : MonoBehaviour
{

    public BodyTracker bodyTracker;

    void OnDrawGizmos()
    {
        Dictionary<ulong, Kinect.Body> trackedIds = null;
        if (bodyTracker != null)
        {
            trackedIds = bodyTracker.TrackedBodies();
        }
        if (trackedIds != null)
        {
            foreach (ulong id in trackedIds.Keys)
            {
                Kinect.Body body = trackedIds[id];
                for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                {
                    Kinect.Joint joint = body.Joints[jt];
                    switch (joint.TrackingState)
                    {
                        case Kinect.TrackingState.Tracked:
                            Gizmos.color = Color.green; break;
                        case Kinect.TrackingState.Inferred:
                            Gizmos.color = Color.yellow; break;
                        case Kinect.TrackingState.NotTracked:
                            Gizmos.color = Color.red; break;
                    }
                    Vector3 jointPosition = BodyTracker.CameraSpacePointToVector3(joint.Position);
                    Gizmos.DrawSphere(jointPosition, 0.01f);
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(BodyTracker.BodyPosition(body), 0.1f);
            }
        }
    }

}
