using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

public class Scenery : MonoBehaviour
{
    
    public Vector3 movementInput = Vector3.zero;
    public float minImageDepth = -1;
    public float maxImageDepth = 0;

    [Header("Kinect input")]
    public bool useKinectInput = false;
    public Vector3 inputMultiplier = Vector3.one;
    public Vector3 inputOffset = Vector3.zero;
    public float inputSmoothing = 10;
    public BodyTracker bodyTracker;

    SceneryImage[] sceneryImages;
    Vector3 previousMousePosition;


    void Start()
    {
        previousMousePosition = Input.mousePosition;
        sceneryImages = GetComponentsInChildren<SceneryImage>();
        
        if (bodyTracker == null)
        {
            Debug.Log("No body tracker assigned!");
        }
    }
    void OnValidate()
    {
        sceneryImages = GetComponentsInChildren<SceneryImage>();
    }

    void Update()
    {
        // Get Kinect input
        if (useKinectInput && bodyTracker != null)
        {
            Vector3 position = inputOffset;
            position += BodyTracker.BodyPosition(bodyTracker.NearestBody());
            position = Vector3.Scale(position, inputMultiplier);
            movementInput = Vector3.Slerp(movementInput, position, Time.deltaTime * inputSmoothing);
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

            // Set the image's new position and scale according to input and depth.
            Vector3 position = movementInput * depthMultiplier;
            position.z = 0;
            sceneryImage.SetRelativePosition(position);

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
