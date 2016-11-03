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
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();

        previousMousePosition = Input.mousePosition;

        if (bodyTracker == null)
        {
            Debug.Log("No body tracker assigned!");
        }

        // Load a scenery from the path given as a command line argument
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            string path = args[1];
            if (System.IO.File.Exists(path))
            {
                LoadScenery(args[1]);
            }
        }
        // Optionally disable kinect input from command line argument
        if (args.Length >= 3)
        {
            if (args[2] == "nokinect")
            {
                useKinectInput = false;
            }
            else
            {
                useKinectInput = true;
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


        Camera camera = Camera.main;
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;
        Vector3 cameraPos = camera.transform.position;
        Rect cameraRectangle = new Rect(cameraPos.x - halfWidth, cameraPos.y - halfHeight, halfWidth * 2, halfHeight * 2);

        float maxRightOverflow = 0;
        float maxLeftOverflow = 0;
        float maxUpOverflow = 0;
        float maxDownOverflow = 0;

        foreach (MovableSceneryObject movableSceneryObject in movableSceneryObjects)
        {
            movableSceneryObject.SetRelativePosition(SceneryObjectRelativePosition(movableSceneryObject.transform.localPosition.z, movementInput));
            movableSceneryObject.SetRelativeScale(SceneryObjectRelativeScale(movableSceneryObject.transform.localPosition.z, movementInput.z));
            SceneryImage sceneryImage = movableSceneryObject.GetComponent<SceneryImage>();
            if (sceneryImage != null)
            {
                Rect minimumRectangle = sceneryImage.MinimumRectangle();

                float depth = movableSceneryObject.transform.position.z;
                Vector3 relativePosition = movableSceneryObject.GetRelativePosition();

                if (sceneryImage.restrictHorizontalMovement)
                {
                    float rightOverflow = InverseSceneryObjectRelativePosition(depth, Vector3.right * (cameraRectangle.xMax - minimumRectangle.xMax)).x;
                    if (rightOverflow > maxRightOverflow)
                    {
                        maxRightOverflow = rightOverflow;
                    }
                    float leftOverflow = InverseSceneryObjectRelativePosition(depth, Vector3.left * (cameraRectangle.x - minimumRectangle.x)).x;
                    if (leftOverflow > maxLeftOverflow)
                    {
                        maxLeftOverflow = leftOverflow;
                    }
                }
                if (sceneryImage.restrictVerticalMovement)
                {
                    float upOverflow = InverseSceneryObjectRelativePosition(depth, Vector3.up * (cameraRectangle.yMax - minimumRectangle.yMax)).y;
                    if (upOverflow > maxUpOverflow)
                    {
                        maxUpOverflow = upOverflow;
                    }
                    float downOverflow = InverseSceneryObjectRelativePosition(depth, Vector3.down * (cameraRectangle.y - minimumRectangle.y)).y;
                    if (downOverflow > maxDownOverflow)
                    {
                        maxDownOverflow = downOverflow;
                    }
                }
            }
        }
        
        Vector3 fixedMovementInput = movementInput;
        fixedMovementInput -= Vector3.left * maxRightOverflow;
        fixedMovementInput -= Vector3.right * maxLeftOverflow;
        fixedMovementInput -= Vector3.down * maxUpOverflow;
        fixedMovementInput -= Vector3.up * maxDownOverflow;
        print("Right: " + maxRightOverflow + ", Left: " + maxLeftOverflow + ", Up: " + maxUpOverflow + ", Down: " + maxDownOverflow);
        if (!Vector3.Equals(movementInput, fixedMovementInput))
        {
            foreach (MovableSceneryObject movableSceneryObject in movableSceneryObjects)
            {
                movableSceneryObject.SetRelativePosition(SceneryObjectRelativePosition(movableSceneryObject.transform.position.z, fixedMovementInput));
            }
        }
        movementInput = fixedMovementInput;

        previousMousePosition = Input.mousePosition;
    }

    // Returns a number that is used to multiply transformation of movable scenery objects at the given depth
    float DepthMultiplier(float depth)
    {
        return 1 - Mathf.InverseLerp(minImageDepth, maxImageDepth, depth);
    }

    // Returns a new relative position for a movable scenery object according to given depth and input
    Vector3 SceneryObjectRelativePosition(float depth, Vector2 input)
    {
        Vector3 position = input * DepthMultiplier(depth);
        return position;
    }
    // Returns a new relative scale for a movable scenery object according to given depth and input
    Vector3 SceneryObjectRelativeScale(float depth, float input)
    {
        Vector3 scale = Vector3.one * (1 + DepthMultiplier(depth) * input);
        return scale;
    }
    Vector2 InverseSceneryObjectRelativePosition(float depth, Vector3 position)
    {
        float depthMultiplier = DepthMultiplier(depth);
        if (depthMultiplier != 0)
        {
            Vector2 input = position / depthMultiplier;
            return input;
        }
        return Vector2.zero;
    }
    float InverseSceneryObjectRelativeScale(float depth, Vector3 scale)
    {
        float depthMultiplier = DepthMultiplier(depth);
        if (depthMultiplier != 0)
        {
            float input = (scale.z - 1) / depthMultiplier;
            return input;
        }
        return 0;
    }


    void OnTransformChildrenChanged()
    {
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
    }
    void OnValidate()
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
        if (System.IO.File.Exists(sourcePath))
        {
            filePath = sourcePath;
            string json = System.IO.File.ReadAllText(sourcePath);
            SetData(JsonUtility.FromJson<SceneryData>(json));
        }
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

    void OnDrawGizmos()
    {
        foreach (var o in movableSceneryObjects)
        {
            SceneryImage img = o.GetComponent<SceneryImage>();
            if (img != null)
            {
                Rect rect = img.MinimumRectangle();
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(new Vector3(rect.x, rect.y, 0), 0.5f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(new Vector3(rect.xMax, rect.yMax, 0), 0.5f);
            }
        }

        Camera camera = Camera.main;
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;
        Vector3 cameraPos = camera.transform.position;
        Rect cameraRectangle = new Rect(cameraPos.x - halfWidth, cameraPos.y - halfHeight, halfWidth * 2, halfHeight * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(cameraRectangle.x, cameraRectangle.y, 0), 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(cameraRectangle.xMax, cameraRectangle.yMax, 0), 0.5f);
    }
}