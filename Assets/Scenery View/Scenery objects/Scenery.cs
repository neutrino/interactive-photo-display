using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

// The serialized data object for saving and loading the scenery
[System.Serializable]
public class SceneryData : SceneryObjectData
{
    public SceneryImageData[] images;
    public SceneryTextData[] texts;
}



public class Scenery : MonoBehaviour, SceneryObject
{

    public GameObject sceneryImagePrefab;
    public GameObject sceneryTextPrefab;

    [Space]
    public Vector3 movementInput = Vector3.zero;
    public float minImageDepth = -1;
    public float maxImageDepth = 0;

    [Header("Kinect input")]
    public bool useKinectInput = false;
    public Vector3 inputMultiplier = Vector3.one;
    public Vector3 inputOffset = Vector3.zero;
    public float inputSmoothing = 10;
    public BodyTracker bodyTracker;


    private MovableSceneryObject[] movableSceneryObjects = new MovableSceneryObject[0];
    private string filePath;

    private Vector3 previousMousePosition;

    void Start()
    {
        previousMousePosition = Input.mousePosition;

        if (bodyTracker == null)
        {
            Debug.Log("No body tracker assigned!");
        }

        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            string path = args[1];
            if (System.IO.File.Exists(path))
            {
                LoadScenery(args[1]);
            }
        }
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
            movementInput += (Input.mousePosition - previousMousePosition) / 200.0f;
        }

        // Move each scenery image to a new position depending on input and their depth.
        foreach (MovableSceneryObject movableSceneryObject in movableSceneryObjects)
        {
            // The smaller the depth (closer to the camera), the faster movement.
            float depth = movableSceneryObject.transform.localPosition.z;
            float depthMultiplier = 1 - Mathf.InverseLerp(minImageDepth, maxImageDepth, depth);
            
            // Set the image's new position and scale according to input and depth.
            Vector3 position = movementInput * depthMultiplier;
            position.z = 0;
            movableSceneryObject.SetRelativePosition(position);

            Vector3 scale = Vector3.one * (1 + depthMultiplier * movementInput.z);
            movableSceneryObject.SetRelativeScale(scale);
            
        }

        previousMousePosition = Input.mousePosition;
    }

    void OnTransformChildrenChanged()
    {
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
    }

    public string FilePath()
    {
        return filePath;
    }

    public void SaveScenery(string targetPath)
    {
        // Move movable objects back to their starting positions
        foreach (MovableSceneryObject mso in movableSceneryObjects)
        {
            mso.SetRelativePosition(Vector3.zero);
            mso.SetRelativeScale(Vector3.zero);
        }
        
        string json = JsonUtility.ToJson(GetData());
        System.IO.File.WriteAllText(targetPath, json);
    }
    public void LoadScenery(string sourcePath)
    {
        filePath = sourcePath;
        string json = System.IO.File.ReadAllText(sourcePath);
        SetData(JsonUtility.FromJson<SceneryData>(json));
    }

    // Return all scenery objects in children excluding this scenery itself
    public SceneryObject[] GetChildSceneryObjects()
    {
        SceneryObject[] allSceneryObjects = GetComponentsInChildren<SceneryObject>();
        SceneryObject[] exclusiveSceneryObjects = new SceneryObject[allSceneryObjects.Length - 1];
        int i = 0;
        foreach (SceneryObject sceneryObject in allSceneryObjects)
        {
            if (sceneryObject != this)
            {
                exclusiveSceneryObjects[i++] = sceneryObject;
            }
        }
        return exclusiveSceneryObjects;
    }


    // Implementing SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        SceneryData sceneryData = new SceneryData();

        SceneryImage[] sceneryImages = GetComponentsInChildren<SceneryImage>();
        sceneryData.images = new SceneryImageData[sceneryImages.Length];
        int i = 0;
        foreach (SceneryImage sceneryImage in sceneryImages)
        {
            sceneryData.images[i++] = (SceneryImageData)sceneryImage.GetData();
        }
        
        SceneryText[] sceneryTexts = GetComponentsInChildren<SceneryText>();
        sceneryData.texts = new SceneryTextData[sceneryTexts.Length];
        i = 0;
        foreach (SceneryText sceneryText in sceneryTexts)
        {
            sceneryData.texts[i++] = (SceneryTextData)sceneryText.GetData();
        }

        return sceneryData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        SceneryData sceneryData = (SceneryData)sceneryObjectData;
        
        // First destroy any currently contained scenery objects
        foreach (SceneryObject sceneryObject in GetChildSceneryObjects())
        {
            DestroyImmediate(((MonoBehaviour)sceneryObject).gameObject);
        }

        // Create new scenery images from data
        foreach (SceneryImageData sceneryImageData in sceneryData.images)
        {
            SceneryImage sceneryImage = ((GameObject)Instantiate(sceneryImagePrefab, transform)).GetComponent<SceneryImage>();
            if (sceneryImage != null)
            {
                sceneryImage.SetData(sceneryImageData);
            }
        }
        // Create new text elements from data
        foreach (SceneryTextData sceneryTextData in sceneryData.texts)
        {
            SceneryText sceneryText = ((GameObject)Instantiate(sceneryTextPrefab, transform)).GetComponent<SceneryText>();
            if (sceneryText != null)
            {
                sceneryText.SetData(sceneryTextData);
            }
        }
    }
}