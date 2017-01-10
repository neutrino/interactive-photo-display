using UnityEngine;

/*
Configurations is a class that contains session specific settings such as Kinect calibrations and scenery file paths.
It has static methods for loading from or saving to a JSON file.
*/

[System.Serializable]
public class Configurations
{
    /// EventArgs for the Loaded event
    public class LoadedEventArgs : System.EventArgs
    {
        public bool successful;
        public string path;
        public LoadedEventArgs(bool successful, string path)
        {
            this.successful = successful;
            this.path = path;
        }
    }


    // Variables that are read from the JSON file

    public bool useKinectInput = true;
    public Vector3 kinectMultiplier = Vector3.one;
    public Vector3 kinectOffset = Vector3.zero;
    public float kinectSmoothing = 10;

    public float transitionDuration = 3;
    public Color transitionColor = Color.black;

    public bool handAlwaysActive = false;
    public bool multipleHandUsers = true;
    public float handActivationTime = 1;
    public float handDeactivationTime = 1;

    public int OscUserLocationUpdatesPerSecond = 5;
    public string OscOutputIP = "127.0.0.1";
    public int OscOutputPort = 57120;
    public int OscInputPort = 57122;

    public bool displayCameraFeed = false;
    public float cameraFeedAlpha = 1;

    public int resolutionWidth = Screen.currentResolution.width;
    public int resolutionHeight = Screen.currentResolution.height;
    public bool vsync = false;

    public int sceneryChangeInterval = 0;
    public bool shuffleSceneries = false;
    public string[] sceneries;


    // The singleton instance
    private static Configurations instance;

    // Event for when configurations are loaded
    public delegate void LoadedDelegate(object configurations, LoadedEventArgs loadedInfo);
    public static event LoadedDelegate Loaded;

    public static Configurations Instance()
    {
        return instance;
    }

    // Load and return a Configurations object from a .json file
    public static Configurations Load(string path)
    {
        Configurations configurations = null;
        if (System.IO.File.Exists(path))
        {
            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(path);
                try
                {
                    configurations = JsonUtility.FromJson<Configurations>(json);
                    instance = configurations;
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error in parsing the configuration json file: \"" + e.Message + "\"");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Error in reading the configuration file: \"" + e.Message + "\"");
            }
        }
        else
        {
            Debug.Log("Error in loading configurations: File \"" + path + "\" doesn't exist.");
        }
        // Call the event
        if (Loaded != null)
        {
            Loaded(configurations, new LoadedEventArgs(configurations != null, path));
        }

        return configurations;
    }
    // Save a Configurations object to a .json file
    public static void Save(Configurations configurations, string path)
    {
        try
        {
            string json = JsonUtility.ToJson(configurations);
            try
            {
                System.IO.File.WriteAllText(path, json);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error in writing configurations to file: \"" + e.Message + "\"");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Error in generating configurations json: \"" + e.Message + "\"");
        }
    }
}