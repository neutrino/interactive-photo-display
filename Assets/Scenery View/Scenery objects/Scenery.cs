using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

[System.Serializable]
public class SceneryData
{
    [System.Serializable]
    public struct SceneryImageList
    {
        public SceneryImageData[] list;
    }
    public SceneryImageList sceneryImages;
}

public class Scenery : MonoBehaviour
{

    public GameObject sceneryImagePrefab;

    [Space]
    public Vector3 movementInput = Vector3.zero;
    public float minImageDepth = -1;
    public float maxImageDepth = 0;

    public SceneryData data = new SceneryData();

    [Header("Kinect input")]
    public bool useKinectInput = false;
    public Vector3 inputMultiplier = Vector3.one;
    public Vector3 inputOffset = Vector3.zero;
    public float inputSmoothing = 10;
    public BodyTracker bodyTracker;

    private string filePath;

    private SceneryImage[] sceneryImages;
    private Vector3 previousMousePosition;

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

        sceneryImages = GetComponentsInChildren<SceneryImage>();

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

    public string FilePath()
    {
        return filePath;
    }

    public SceneryImage[] SceneryImages()
    {
        return sceneryImages;
    }

    public void UpdateData()
    {
        SceneryImage[] sceneryImages = GetComponentsInChildren<SceneryImage>();
        data.sceneryImages.list = new SceneryImageData[sceneryImages.Length];
        int i = 0;
        foreach (SceneryImage sceneryImage in sceneryImages)
        {
            sceneryImage.UpdateData();
            data.sceneryImages.list[i++] = sceneryImage.data;
        }
    }
    public void UpdateFromData()
    {
        SceneryImage[] sceneryImages = GetComponentsInChildren<SceneryImage>();
        foreach (SceneryImage sceneryImage in sceneryImages)
        {
            DestroyImmediate(sceneryImage.gameObject);
        }
        string directory = System.IO.Path.GetDirectoryName(filePath);
        foreach (SceneryImageData sceneryImageData in data.sceneryImages.list)
        {
            Vector3 pos = new Vector3(sceneryImageData.x, sceneryImageData.y, sceneryImageData.z);
            Quaternion rot = Quaternion.Euler(0, 0, sceneryImageData.rotation);
            SceneryImage sceneryImage = ((GameObject)Instantiate(sceneryImagePrefab, transform)).GetComponent<SceneryImage>();
            sceneryImage.data = sceneryImageData;
            sceneryImage.UpdateFromData();
        }

        sceneryImages = GetComponentsInChildren<SceneryImage>();
    }

    public void SaveScenery(string targetPath)
    {
        UpdateData();
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(targetPath, json);
    }
    public void LoadScenery(string sourcePath)
    {
        filePath = sourcePath;
        string json = System.IO.File.ReadAllText(sourcePath);
        data = JsonUtility.FromJson<SceneryData>(json);
        UpdateFromData();
    }
}









