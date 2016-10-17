using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

public class Scenery : MonoBehaviour
{
    
    public Vector3 movementInput = Vector3.zero;
    public Vector3 movementAmount = Vector3.one;
    public Vector3 movementOffset = Vector3.zero;

    [Space]
    public float minImageDepth = -1;
    public float maxImageDepth = 0;

    [Header("Kinect input")]
    public bool useKinectInput = false;
    public BodySourceView bodySourceView;
    public Kinect.JointType trackedJointType = Kinect.JointType.Head;


    SceneryImage[] sceneryImages;
    Vector3 previousMousePosition;


    void Start()
    {
        previousMousePosition = Input.mousePosition;
        sceneryImages = GetComponentsInChildren<SceneryImage>();
    }
    void OnValidate()
    {
        sceneryImages = GetComponentsInChildren<SceneryImage>();
    }

    void Update()
    {
        // Get kinect input
        if (useKinectInput && bodySourceView != null)
        {
            Kinect.Joint[] joints = bodySourceView.GetJoints(trackedJointType);
            if (joints.Length > 0)
            {
                movementInput.x = -joints[0].Position.X;
                movementInput.y = -joints[0].Position.Y;
            }
        }
        // Get mouse input
        else if (Input.GetMouseButton(0))
        {
            movementInput += (Input.mousePosition - previousMousePosition) / 500.0f;
        }

        // Move each scenery image to a new position depending on input and their depth.
        foreach (var sceneryImage in sceneryImages)
        {
            // The smaller the depth (closer to the camera), the faster movement.
            float depth = sceneryImage.transform.localPosition.z;
            float depthMultiplier = 1 - Mathf.InverseLerp(minImageDepth, maxImageDepth, depth);

            // Set the image's new position according to offset, input, panning amount and depth.
            Vector3 movement = (Vector3.Scale(movementInput, movementAmount) + movementOffset) * depthMultiplier;
            sceneryImage.SetRelativePosition(movement);

            Vector3 scale = Vector3.one * (1 + depthMultiplier * movementInput.z);
            sceneryImage.SetRelativeScale(scale);
        }

        previousMousePosition = Input.mousePosition;
    }

    public SceneryImage[] SceneryImages()
    {
        return sceneryImages;
    }
}
