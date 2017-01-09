using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityOSC;

public class OSCMessenger : MonoBehaviour
{

    private const string clientId = "IPD";

    private BodyTracker bodyTracker;
    private SceneryQueue sceneryQueue;

    private int userLocationUpdatesPerSecond = 2;
    private float userLocationUpdateTimer = 0;
    private List<string> userLocation = new List<string>();

    private bool initialized = false;

    private bool preparedToChangeScenery = false;
    private int nextSceneryNumber = 0;

    void Awake()
    {
        Configurations.Loaded += ConfigurationsLoaded;
    }

    void Update()
    {
        if (initialized)
        {
            // Send user locations with a certain time interval
            if (userLocationUpdateTimer > 0)
            {
                userLocationUpdateTimer -= Time.deltaTime;
            }
            else
            {
                int userLocationUpdatesPerSecond = 5;
                Configurations configs = Configurations.Instance();
                if (configs != null)
                {
                    userLocationUpdatesPerSecond = configs.OscUserLocationUpdatesPerSecond;
                }
                userLocationUpdateTimer = 1f / (float)userLocationUpdatesPerSecond;
                SendUserLocations();
            }

            // If preparedToChangeScenery is flagged (in the PacketReceived event handler), change scenery
            if (preparedToChangeScenery)
            {
                preparedToChangeScenery = false;
                StartCoroutine(sceneryQueue.ChangeScenery(nextSceneryNumber));
            }
        }
    }

    private void ConfigurationsLoaded(object configurations, Configurations.LoadedEventArgs loadedInfo)
    {
        initialized = Initialize();
    }

    // Create server and client and start listening to events
    public bool Initialize()
    {
        Configurations configs = Configurations.Instance();
        if (configs != null)
        {
            OSCHandler.Instance.CreateServer("Server", configs.OscInputPort);
            OSCHandler.Instance.CreateClient("IPD", System.Net.IPAddress.Parse(configs.OscOutputIP), configs.OscOutputPort);

            // Listen to packet receiving events
            foreach (ServerLog serverLog in OSCHandler.Instance.Servers.Values)
            {
                OSCServer server = serverLog.server;
                server.PacketReceivedEvent += PacketReceived;
            }

            // Listen to body (user) entering and leaving events
            bodyTracker = FindObjectOfType<BodyTracker>();
            if (bodyTracker != null)
            {
                bodyTracker.BodyEntered += BodyEntered;
                bodyTracker.BodyLeft += BodyLeft;
            }

            // Listen to scenery changing events
            sceneryQueue = FindObjectOfType<SceneryQueue>();
            if (sceneryQueue != null)
            {
                sceneryQueue.SceneryChanged += SceneryChanged;
            }

            // Listen to pop-up appearing and disappearing events
            SceneryPopUp.PopUpShown += PopUpShown;
            SceneryPopUp.PopUpHidden += PopUpHidden;

            return true;
        }
        return false;
    }

    // Event handler for receiving packets
    private void PacketReceived(OSCServer server, OSCPacket packet)
    {
        // Change scenery if a packet has been received at /scene/change
        if (packet.Address == "/scene/change")
        {
            int sceneryNumber = -1;
            try
            {
                sceneryNumber = (int)packet.Data[0];
            }
            catch (System.Exception e)
            {
            }

            if (sceneryNumber >= 0)
            {
                // This packet receiving event handler is evidently called in another thread
                // and coroutines can't be started outside the main thread. Let's just flag
                // preparedToChangeScenery so that in the main thread (in Update()) the coroutine
                // will be started.
                preparedToChangeScenery = true;
                nextSceneryNumber = sceneryNumber;
            }
            
        }
    }

    // Send all user (body) locations via OSC
    private void SendUserLocations()
    {
        foreach (Kinect.Body body in bodyTracker.TrackedBodies().Values)
        {
            // The UnityOSC (or OSC in general?) library allows for only one type of data to be sent in one message.
            // Therefore both the user id and position values are sent as strings.
            Vector3 bodyPosition = BodyTracker.BodyPosition(body);
            userLocation.Clear();
            userLocation.Add(body.TrackingId.ToString());
            userLocation.Add(bodyPosition.x.ToString());
            userLocation.Add(bodyPosition.y.ToString());
            userLocation.Add(bodyPosition.z.ToString());
            OSCHandler.Instance.SendMessageToClient<string>(clientId, "/user/location", userLocation);
        }
    }

    // Event handler for when a new wbody (user) enters
    private void BodyEntered(object bodyTracker, BodyTracker.BodyEnteredEventArgs bodyEnteredInfo)
    {
        // TrackingId must be sent as a string because ulong isn't supported in OSC messages
        string id = bodyEnteredInfo.body.TrackingId.ToString();
        OSCHandler.Instance.SendMessageToClient<string>(clientId, "/user/new", id);
    }
    // Event handler for when a body (user) leaves
    private void BodyLeft(object bodyTracker, BodyTracker.BodyLeftEventArgs bodyLeftInfo)
    {
        // TrackingId must be sent as a string because ulong isn't supported in OSC messages
        string id = bodyLeftInfo.bodyTrackingId.ToString();
        OSCHandler.Instance.SendMessageToClient<string>(clientId, "/user/lost", id);
    }
    // Event handler for when the scenery has changed
    private void SceneryChanged(object sceneryQueue, SceneryQueue.SceneryChangedEventArgs sceneryChangedInfo)
    {
        int sceneryNumber = sceneryChangedInfo.newSceneryNumber;
        OSCHandler.Instance.SendMessageToClient<int>(clientId, "/scene/changed", sceneryNumber);
    }
    // Event handler for when a pop-up has appeared
    private void PopUpShown(object sceneryPopUp, SceneryPopUp.PopUpShownEventArgs sceneryPopUpInfo)
    {
        SceneryPopUp popUp = (SceneryPopUp)sceneryPopUp;
        string fullText = popUp.GetText();
        string info = fullText;
        if (fullText.Length > 64)
        {
            info = fullText.Substring(0, 64);
        }
        OSCHandler.Instance.SendMessageToClient<string>(clientId, "/information/shown", info);
    }
    // Event handler for when a pop-up has disappeared
    private void PopUpHidden(object sceneryPopUp, SceneryPopUp.PopUpHiddenEventArgs sceneryPopUpInfo)
    {
        SceneryPopUp popUp = (SceneryPopUp)sceneryPopUp;
        string fullText = popUp.GetText();
        string info = fullText;
        if (fullText.Length > 64)
        {
            info = fullText.Substring(0, 64);
        }
        OSCHandler.Instance.SendMessageToClient<string>(clientId, "/information/hidden", info);
    }
}
