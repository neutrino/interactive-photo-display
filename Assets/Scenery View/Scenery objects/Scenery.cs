using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

// The serialized data object for saving and loading the scenery
[System.Serializable]
public class SceneryData : SceneryObjectData
{
    public int cameraHeight = 720;
    public SceneryImageData[] images;
    public SceneryTextData[] texts;
    public SceneryPopUpData[] popUps;
}



public class Scenery : MonoBehaviour, SceneryObject
{

    public GameObject sceneryImagePrefab;
    public GameObject sceneryTextPrefab;
    public GameObject sceneryPopUpPrefab;
    
    [Space]
    public int cameraPixelsPerUnits = 100;

    [Space]
    public Vector3 movementInput = Vector3.zero;
    public float minImageDepth = -1;
    public float maxImageDepth = 0;

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
            bodyTracker = FindObjectOfType<BodyTracker>();
            if (bodyTracker == null)
            {
                Debug.Log("Can't find a BodyTracker!");
            }
        }
    }
    void OnTransformChildrenChanged()
    {
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
    }
    void OnValidate()
    {
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
    }
    
    void Update()
    {
        Configurations configs = Configurations.Instance();
        if (configs != null)
        {
            // Get Kinect input
            if (configs.useKinectInput && bodyTracker != null)
            {
                Kinect.Body body = bodyTracker.ActiveControllerBody();
                Vector3 position = configs.KinectOffset();
                if (body != null)
                {
                    position += BodyTracker.BodyPosition(body);
                }
                position = Vector3.Scale(position, configs.KinectMultiplier());
                movementInput = Vector3.Slerp(movementInput, position, Time.deltaTime * configs.kinectSmoothing);
            }
            // Get mouse input
            else if (Input.GetMouseButton(0))
            {
                movementInput += (Input.mousePosition - previousMousePosition) / 50.0f;
            }
            movementInput.z = Mathf.Max(0, movementInput.z);


            // First move and scale the objects ignoring camera restrictions
            TransformMovableSceneryObjects(movementInput);
            // According to new objects' positions calculate a new fixed input vector that takes restrictions to account
            Vector3 fixedMovementInput = RestrictedMovementInput(movementInput);
            if (!Vector3.Equals(movementInput, fixedMovementInput))
            {
                // Move the objects again to their new positions according to fixed input
                TransformMovableSceneryObjects(fixedMovementInput);
            }

            if (!configs.useKinectInput)
            {
                movementInput = fixedMovementInput;
            }

            previousMousePosition = Input.mousePosition;
        }
    }

    // Move and scale all movable scenery objects according to input. Don't scale all the way to 0.
    private void TransformMovableSceneryObjects(Vector3 input)
    {
        foreach (MovableSceneryObject movableSceneryObject in movableSceneryObjects)
        {
            movableSceneryObject.SetRelativePosition(SceneryObjectRelativePosition(movableSceneryObject.transform.localPosition.z, input));
            Vector3 scale = SceneryObjectRelativeScale(movableSceneryObject.transform.localPosition.z, input.z);
            scale.x = Mathf.Max(0.0001f, scale.x);
            scale.y = Mathf.Max(0.0001f, scale.y);
            scale.z = Mathf.Max(0.0001f, scale.z);
            movableSceneryObject.SetRelativeScale(scale);
        }
    }

    // Get the ortographic camera's corners in world space ignoring the z-axis
    private Rect CameraRectangle(Camera camera)
    {
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;
        Vector3 cameraPos = camera.transform.position;
        Rect rect = new Rect(cameraPos.x - halfWidth, cameraPos.y - halfHeight, halfWidth * 2, halfHeight * 2);
        return rect;
    }

    // Calculate an input vector that takes camera movement restrictions to account according to object's positions
    private Vector3 RestrictedMovementInput(Vector3 input)
    {
        Rect cameraRectangle = CameraRectangle(Camera.main);
        float maxRightOverflow = 0;
        float maxLeftOverflow = 0;
        float maxUpOverflow = 0;
        float maxDownOverflow = 0;
        foreach (MovableSceneryObject movableSceneryObject in movableSceneryObjects)
        {
            SceneryImage sceneryImage = movableSceneryObject.GetComponent<SceneryImage>();
            if (sceneryImage != null)
            {
                Rect minimumRectangle = sceneryImage.MinimumUprightRectangle();
                float depth = movableSceneryObject.transform.position.z;

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
        if (!(maxRightOverflow != 0 && maxLeftOverflow != 0))
        {
            fixedMovementInput -= Vector3.left * maxRightOverflow;
            fixedMovementInput -= Vector3.right * maxLeftOverflow;
        }
        else
        {
            fixedMovementInput.x = 0;
        }
        if (!(maxUpOverflow != 0 && maxDownOverflow != 0))
        {
            fixedMovementInput -= Vector3.down * maxUpOverflow;
            fixedMovementInput -= Vector3.up * maxDownOverflow;
        }
        else
        {
            fixedMovementInput.y = 0;
        }

        return fixedMovementInput;
    }

    // Returns a number that is used to multiply transformation of movable scenery objects at the given depth
    private float DepthMultiplier(float depth)
    {
        return 1 - Mathf.InverseLerp(minImageDepth, maxImageDepth, depth);
    }

    // Returns a new relative position for a movable scenery object according to given depth and input
    private Vector3 SceneryObjectRelativePosition(float depth, Vector2 input)
    {
        Vector3 position = input * DepthMultiplier(depth);
        return position;
    }
    // Returns a new relative scale for a movable scenery object according to given depth and input
    private Vector3 SceneryObjectRelativeScale(float depth, float input)
    {
        Vector3 scale = Vector3.one * (1 + DepthMultiplier(depth) * input);
        return scale;
    }
    private Vector2 InverseSceneryObjectRelativePosition(float depth, Vector3 position)
    {
        float depthMultiplier = DepthMultiplier(depth);
        if (depthMultiplier != 0)
        {
            Vector2 input = position / depthMultiplier;
            return input;
        }
        return Vector2.zero;
    }
    private float InverseSceneryObjectRelativeScale(float depth, Vector3 scale)
    {
        float depthMultiplier = DepthMultiplier(depth);
        if (depthMultiplier != 0)
        {
            float input = (scale.z - 1) / depthMultiplier;
            return input;
        }
        return 0;
    }
    
    public string FilePath()
    {
        return filePath;
    }

    // Save the scenery's info to a json file
    public void SaveScenery(string targetPath)
    {
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
        // Move movable objects back to their starting positions
        foreach (MovableSceneryObject mso in movableSceneryObjects)
        {
            mso.SetRelativePosition(Vector3.zero);
            mso.SetRelativeScale(Vector3.zero);
        }

        string json = JsonUtility.ToJson(GetData());
        System.IO.File.WriteAllText(targetPath, json);
    }
    // Load the scenery from a json file
    public void LoadScenery(string sourcePath)
    {
        if (System.IO.File.Exists(sourcePath))
        {
            // Find the absolute path for future reference, because the scenery's file path could've be given relative to the .exe
            filePath = System.IO.Path.GetFullPath(sourcePath);
            string json = System.IO.File.ReadAllText(sourcePath);
            SetData(JsonUtility.FromJson<SceneryData>(json));
        }
        movableSceneryObjects = GetComponentsInChildren<MovableSceneryObject>();
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

        SceneryPopUp[] sceneryPopUps = GetComponentsInChildren<SceneryPopUp>();
        sceneryData.popUps = new SceneryPopUpData[sceneryPopUps.Length];
        i = 0;
        foreach (SceneryPopUp sceneryPopUp in sceneryPopUps)
        {
            sceneryData.popUps[i++] = (SceneryPopUpData)sceneryPopUp.GetData();
        }

        return sceneryData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        SceneryData sceneryData = (SceneryData)sceneryObjectData;

        // Set the camera's size
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = sceneryData.cameraHeight / cameraPixelsPerUnits;
        }
        
        // First destroy any currently contained scenery objects
        foreach (SceneryObject sceneryObject in GetComponentsInChildren<SceneryObject>())
        {
            if (sceneryObject != (SceneryObject)this)
            {
                DestroyImmediate(((MonoBehaviour)sceneryObject).gameObject);
            }
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

        // Create new pop-up elements from data
        foreach (SceneryPopUpData sceneryPopUpData in sceneryData.popUps)
        {
            SceneryPopUp sceneryPopUp = ((GameObject)Instantiate(sceneryPopUpPrefab, transform)).GetComponent<SceneryPopUp>();
            if (sceneryPopUp != null)
            {
                sceneryPopUp.SetData(sceneryPopUpData);
            }
        }
    }
}