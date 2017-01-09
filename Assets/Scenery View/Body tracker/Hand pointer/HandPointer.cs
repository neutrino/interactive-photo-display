using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

public class HandPointer : MonoBehaviour
{
    public enum Side
    {
        Right,
        Left
    }

    public float popupActivationDistance;
    public Texture textureHandOpen;
    public Texture textureHandClosed;
    public float handTextureScale = 1;
    public float movementSmoothing = 10;

    private Kinect.Body body;
    private ulong bodyTrackingId;
    private BodyTracker bodyTracker;
    private Side side;
    private Vector2 positionOnScreen;

    private bool active = false;
    private bool keptUp = false;
    private float activationTimer = 0;

    private Kinect.HandState latestValidHandState = Kinect.HandState.Open;
    private Kinect.HandState previousValidHandState = Kinect.HandState.Open;

    void Start()
    {
        bodyTracker = FindObjectOfType<BodyTracker>();
    }

    void Update()
    {
        Kinect.HandState hs = HandState();
        if (hs == Kinect.HandState.Open || hs == Kinect.HandState.Closed || hs == Kinect.HandState.Lasso)
        {
            latestValidHandState = hs;
        }

        Vector2 targetPosition = TargetPositionOnScreen();
        if (targetPosition != Vector2.zero)
        {
            positionOnScreen += (TargetPositionOnScreen() - positionOnScreen) * Time.deltaTime * movementSmoothing;
        }

        Configurations configs = Configurations.Instance();

        float activationTime = 1.0f;
        float deactivationTime = 1.0f;
        if (configs != null)
        {
            activationTime = configs.handActivationTime;
            deactivationTime = configs.handDeactivationTime;
        }
        
        // Keep track of time and change the value of keptUp according to how long the hand has been kept up
        if (!keptUp)
        {
            // When the hand is kept up, take time and finally declare the hand as kept up
            if (IsUp())
            {
                if (activationTimer < activationTime)
                {
                    activationTimer += Time.deltaTime;
                }
                else
                {
                    keptUp = true;
                    activationTimer = 0;
                }
            }
        }
        else
        {
            // When the hand is kept down, take time and finally declare the hand as kept down
            if (!IsUp())
            {
                if (activationTimer < deactivationTime)
                {
                    activationTimer += Time.deltaTime;
                }
                else
                {
                    keptUp = false;
                    activationTimer = 0;
                }
            }
            else
            {
                activationTimer = 0;
            }
        }

        // Determine if the hand is active according to keptUp and a couple of exceptions
        active = keptUp;
        if (configs != null)
        {
            if (configs.handAlwaysActive)
            {
                active = true;
            }
            if (!configs.multipleHandUsers && bodyTracker.ActiveControllerBody() != body)
            {
                active = false;
            }
        }

        // When active, refresh visibility timers, and if the hand has been closed, toggle any pop-up message's visibility that's located at the hand's position on screen
        if (active)
        {
            Vector2 screenPosition = PositionOnScreen();
            foreach (SceneryPopUp popup in FindObjectsOfType<SceneryPopUp>())
            {
                if (popup.ScreenPointCloseEnough(screenPosition))
                {
                    // Reset timer
                    popup.ResetTimer();

                    popup.HandOnIcon();

                    if (previousValidHandState == Kinect.HandState.Open && (latestValidHandState == Kinect.HandState.Closed || latestValidHandState == Kinect.HandState.Lasso))
                    {
                        // Toggle visibility
                        popup.ToggleVisibility();
                    }
                }
            }
        }

        previousValidHandState = latestValidHandState;
    }

    public Kinect.Joint Joint()
    {
        if (body != null)
        {
            return body.Joints[JointType()];
        }
        else
        {
            throw new System.NullReferenceException();
        }
    }
    public Kinect.JointType JointType()
    {
        switch (side)
        {
            case Side.Left: return Kinect.JointType.HandLeft;
            case Side.Right: return Kinect.JointType.HandRight;
            default: return Kinect.JointType.HandRight;
        }
    }
    public Kinect.HandState HandState()
    {
        if (body != null)
        {
            switch (side)
            {
                case Side.Left: return body.HandLeftState;
                case Side.Right: return body.HandRightState;
            }
        }
        return Kinect.HandState.Unknown;
    }
    public Kinect.HandState LastValidHandState()
    {
        return latestValidHandState;
    }

    public void LinkToBody(Kinect.Body body, Side side)
    {
        this.body = body;
        this.bodyTrackingId = body.TrackingId;
        this.side = side;
    }

    public Kinect.Body Body()
    {
        return body;
    }
    public ulong BodyTrackingId()
    {
        return bodyTrackingId;
    }

    public Vector2 PositionOnScreen()
    {
        return positionOnScreen;
    }
        
    public Vector2 TargetPositionOnScreen()
    {
        Vector2 screenPosition = Vector2.zero;
        if (body != null && bodyTracker != null)
        {
            Vector2 position = bodyTracker.JointPositionOnScreen(body, JointType());
            screenPosition = Vector2.Scale(position, new Vector2(Screen.width, Screen.height));
        }
        return screenPosition;
    }

    // Returns true if the hand is above its corresponding arm's elbow
    private bool IsUp()
    {
        bool isUp = false;
        if (body != null)
        {
            Kinect.JointType elbowJointType;
            switch (side)
            {
                case Side.Left:
                    elbowJointType = Kinect.JointType.ElbowLeft; break;
                case Side.Right:
                    elbowJointType = Kinect.JointType.ElbowRight; break;
                default:
                    elbowJointType = Kinect.JointType.ElbowRight; break;
            }
            isUp = body.Joints[JointType()].Position.Y > body.Joints[elbowJointType].Position.Y;
        }
        return isUp;
    }

    void OnGUI()
    {
        // Draw a texture over the hand's position.
        if (active)
        {
            Vector2 textureSize = new Vector2(textureHandOpen.width, textureHandOpen.height) * handTextureScale;
            if (side == Side.Right)
            {
                textureSize.x *= -1;
            }
            Rect position = new Rect(PositionOnScreen() - textureSize / 2.0f, textureSize);
            if (textureHandClosed != null && textureHandOpen != null)
            {
                switch (latestValidHandState)
                {
                    case Kinect.HandState.Open:
                        GUI.DrawTexture(position, textureHandOpen);
                        break;
                    default:
                        GUI.DrawTexture(position, textureHandClosed);
                        break;
                }
            }
        }
    }

}
