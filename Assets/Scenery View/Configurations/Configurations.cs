using UnityEngine;

/*
Configurations is a class that contains session specific settings such as Kinect calibrations and scenery file paths.
It has static methods for loading from or saving to a .json file.
*/

[System.Serializable]
public class Configurations
{
    public bool useKinectInput = true;
    public float kinectMultiplierX = 1;
    public float kinectMultiplierY = 1;
    public float kinectMultiplierZ = 1;
    public float kinectOffsetX = 0;
    public float kinectOffsetY = 0;
    public float kinectOffsetZ = 0;
    public float kinectSmoothing = 10;
    public float transitionDuration = 3;
    public float transitionColorRed = 0;
    public float transitionColorGreen = 0;
    public float transitionColorBlue = 0;
    public bool handAlwaysActive = false;
    public bool multipleHandUsers = true;
    public float handActivationTime = 1;
    public float handDeactivationTime = 1;
    public int OscUserLocationUpdatesPerSecond = 5;
    public string OscOutputIP = "127.0.0.1";
    public int OscOutputPort = 57120;
    public int OscInputPort = 57122;
    public int sceneryChangeInterval = 0;
    public bool shuffleSceneries = false;
    public string[] sceneries;

    private static Configurations instance;

    public static Configurations Instance()
    {
        return instance;
    }
    public static void SetInstance(Configurations configurations)
    {
        instance = configurations;
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

    // Return Kinect multiplier variables as a Vector3
    public Vector3 KinectMultiplier()
    {
        return new Vector3(kinectMultiplierX, kinectMultiplierY, kinectMultiplierZ);
    }
    // Return Kinect offset variables as a Vector3
    public Vector3 KinectOffset()
    {
        return new Vector3(kinectOffsetX, kinectOffsetY, kinectOffsetZ);
    }
}