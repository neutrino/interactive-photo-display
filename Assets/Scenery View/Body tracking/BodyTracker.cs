using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

/*
BodyTracker, with the help of the Kinect library reads Kinect input to track user movements.
*/

public class BodyTracker : MonoBehaviour
{
    public class BodyEnteredEventArgs : System.EventArgs
    {
        public Kinect.Body body;
        public BodyEnteredEventArgs(Kinect.Body body)
        {
            this.body = body;
        }
    }
    public class BodyLeftEventArgs : System.EventArgs
    {
        public ulong bodyTrackingId;
        public BodyLeftEventArgs(ulong bodyTrackingId)
        {
            this.bodyTrackingId = bodyTrackingId;
        }
    }

    public GameObject handPointerPrefab;

    public delegate void BodyEnteredDelegate(object bodyTracker, BodyEnteredEventArgs bodyEnteredInfo);
    public event BodyEnteredDelegate BodyEntered;
    public delegate void BodyLeftDelegate(object bodyTracker, BodyLeftEventArgs bodyLeftInfo);
    public event BodyLeftDelegate BodyLeft;

    private Kinect.KinectSensor kinectSensor;
    private Kinect.BodyFrameReader bodyFrameReader;
    private Kinect.Body[] bodyData;
    private Dictionary<ulong, Kinect.Body> trackedBodies = new Dictionary<ulong, Kinect.Body>();

    private Kinect.Body activeControllerBody;

    void Start()
    {
        StartKinect();

        // Add handlers for body entering and leaving events
        BodyEntered += BodyTracker_BodyEntered;
        BodyLeft += BodyTracker_BodyLeft;
    }

    void OnDestroy()
    {
        CloseKinect();
    }



    // Public methods

    public Dictionary<ulong, Kinect.Body> TrackedBodies()
    {
        return trackedBodies;
    }

    public Kinect.Body ActiveControllerBody()
    {
        return activeControllerBody;
    }

    // Returns the body nearest to the camera.
    public Kinect.Body NearestBody()
    {
        Kinect.Body nearestBody = null;
        if (trackedBodies.Count > 0)
        {
            float nearestBodyDistance = float.PositiveInfinity;
            foreach (Kinect.Body body in trackedBodies.Values)
            {
                Vector3 pos = BodyPosition(body);
                if (pos != Vector3.zero && pos.z < nearestBodyDistance)
                {
                    nearestBody = body;
                    nearestBodyDistance = pos.z;
                }
            }
            return nearestBody;
        }
        return nearestBody;
    }

    // Finds and returns the body with the given TrackingId or null if there is none.
    public Kinect.Body FindBodyWithId(ulong id)
    {
        foreach (Kinect.Body body in trackedBodies.Values)
        {
            if (body.TrackingId == id)
            {
                return body;
            }
        }
        return null;
    }


    // Map a joint's CameraSpacePoint to 2D space with values ranging from 0 to 1
    public Vector2 JointPositionOnScreen(Kinect.Body body, Kinect.JointType jointType)
    {
        Vector2 pointOnScreen = Vector2.zero;
        if (body != null && body.IsTracked)
        {
            Kinect.Joint joint = body.Joints[jointType];
            if (joint.TrackingState == Kinect.TrackingState.Tracked)
            {
                pointOnScreen = PositionOnScreen(joint.Position);
                /*
                Kinect.ColorSpacePoint point = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
                // The Kinect V2's color view's resolution is 1920x1080
                pointOnScreen = new Vector2(point.X / 1920f, point.Y / 1080f);
                */
            }
        }
        return pointOnScreen;
    }
    // Map a CameraSpacePoint to 2D space with values ranging from 0 to 1
    public Vector2 PositionOnScreen(Kinect.CameraSpacePoint position)
    {
        Kinect.ColorSpacePoint screenPoint = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(position);
        return new Vector2(screenPoint.X / 1920f, screenPoint.Y / 1080f);
    }


    // Private methods

    // Open the sensor and its body frame reader
    private void StartKinect()
    {
        // Open the sensor
        kinectSensor = Kinect.KinectSensor.GetDefault();
        if (kinectSensor != null)
        {
            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
            }
        }
        else
        {
            Debug.Log("Unable to open the Kinect sensor!");
        }
        
        // Open the body frame reader
        if (kinectSensor != null)
        {
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            if (bodyFrameReader != null)
            {
                // Add a handler for when a frame arrives from the reader.
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            }
            else
            {
                Debug.Log("Unable to open the body frame reader!");
            }
        }
    }

    // Close the sensor and dispose its body frame reader
    private void CloseKinect()
    {
        if (bodyFrameReader != null)
        {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }
        if (kinectSensor != null)
        {
            if (kinectSensor.IsOpen)
            {
                kinectSensor.Close();
            }
            kinectSensor = null;
        }
    }

    // Handler for the body frame reader's FrameArrived event
    private void BodyFrameReader_FrameArrived(object sender, Kinect.BodyFrameArrivedEventArgs e)
    {
        UpdateBodyData(e.FrameReference.AcquireFrame());
    }

    // Handler for BodyEntered event
    private void BodyTracker_BodyEntered(object bodyTracker, BodyEnteredEventArgs bodyEnteredInfo)
    {
        if (handPointerPrefab != null)
        {
            if (bodyEnteredInfo.body != null)
            {
                HandPointer handPointerLeft = Instantiate(handPointerPrefab).GetComponent<HandPointer>();
                handPointerLeft.LinkToBody(bodyEnteredInfo.body, HandPointer.Side.Left);
                HandPointer handPointerRight = Instantiate(handPointerPrefab).GetComponent<HandPointer>();
                handPointerRight.LinkToBody(bodyEnteredInfo.body, HandPointer.Side.Right);
            }
        }
    }
    // Handler for BodyLeft event
    private void BodyTracker_BodyLeft(object bodyTracker, BodyLeftEventArgs bodyLeftInfo)
    {
        foreach (HandPointer handPointer in FindObjectsOfType<HandPointer>())
        {
            if (handPointer.BodyTrackingId() == bodyLeftInfo.bodyTrackingId)
            {
                Destroy(handPointer.gameObject);
            }
        }
    }

    // Update the body data to match the frame
    private void UpdateBodyData(Kinect.BodyFrame frame)
    {
        if (frame != null)
        {
            if (bodyData == null)
            {
                bodyData = new Kinect.Body[kinectSensor.BodyFrameSource.BodyCount];
            }
            // Note: Don't do anything with bodyData before disposing the frame. This seems to break a lot of things for unknown reasons.
            frame.GetAndRefreshBodyData(bodyData);
            frame.Dispose();
            frame = null;
            UpdateTrackedIds();

            activeControllerBody = NearestBody();
        }
    }

    // Update the dictionary of tracked bodies and their ids. Call events for entered and left bodies.
    private void UpdateTrackedIds()
    {
        // First remove any untracked bodies. Mark bodies to be removed in untrackedIds.
        ulong[] untrackedIds = new ulong[trackedBodies.Count];
        int i = 0;
        foreach (ulong id in trackedBodies.Keys)
        {
            Kinect.Body body = trackedBodies[id];
            if (body == null || !body.IsTracked || System.Array.IndexOf(bodyData, body) == -1)
            {
                // A body has left - Call the event and mark the id for removal
                if (BodyLeft != null)
                {
                    BodyLeft(this, new BodyLeftEventArgs(id));
                }
                untrackedIds[i++] = id;
            }
        }
        // Remove the previously marked bodies.
        foreach (ulong id in untrackedIds)
        {
            if (id != 0)
            {
                trackedBodies.Remove(id);
            }
            else
            {
                break;
            }
        }
        // Add new tracked bodies.
        foreach (Kinect.Body body in bodyData)
        {
            if (body != null && body.IsTracked && !trackedBodies.ContainsKey(body.TrackingId))
            {
                // A body has entered - Call the event and add the id and body to trackedIds.
                if (BodyEntered != null)
                {
                    BodyEntered(this, new BodyEnteredEventArgs(body));
                }
                trackedBodies.Add(body.TrackingId, body);
            }
        }
    }


    // Static methods

    // Return the average body position based on several joints.
    public static Vector3 BodyPosition(Kinect.Body body)
    {
        Vector3 position = Vector3.zero;
        if (body != null)
        {
            Kinect.Joint[] joints = new Kinect.Joint[3];
            int i = 0;
            joints[i++] = body.Joints[Kinect.JointType.SpineShoulder];
            joints[i++] = body.Joints[Kinect.JointType.SpineMid];
            joints[i++] = body.Joints[Kinect.JointType.SpineBase];

            int trackedBodyCount = 0;
            foreach (Kinect.Joint joint in joints)
            {
                if (joint.TrackingState == Kinect.TrackingState.Tracked)
                {
                    position += CameraSpacePointToVector3(joint.Position);
                    ++trackedBodyCount;
                }
            }
            if (trackedBodyCount > 0)
            {
                position /= trackedBodyCount;
            }
        }
        return position;
    }

    // Return the body's specific joint's position
    public static Vector3 JointPosition(Kinect.Body body, Kinect.JointType jointType)
    {
        if (body != null && body.IsTracked)
        {
            Kinect.Joint joint = body.Joints[jointType];
            if (joint.TrackingState == Kinect.TrackingState.Tracked)
            {
                return CameraSpacePointToVector3(joint.Position);
            }
        }
        return Vector3.zero;
    }

    // Convert CameraSpacePoint to a Vector3
    public static Vector3 CameraSpacePointToVector3(Kinect.CameraSpacePoint position)
    {
        return new Vector3(position.X, position.Y, position.Z);
    }
    // Convert Vector3 to a CameraSpacePoint
    public static Kinect.CameraSpacePoint Vector3ToCameraSpacePoint(Vector3 position)
    {
        Kinect.CameraSpacePoint point = new Kinect.CameraSpacePoint();
        point.X = position.x;
        point.Y = position.y;
        point.Z = position.z;
        return point;
    }

}
