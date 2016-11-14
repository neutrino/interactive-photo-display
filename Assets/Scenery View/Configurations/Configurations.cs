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
    public int cameraHeight = 720;
    public int sceneryChangeInterval = 0;
    public bool shuffleSceneries = false;
    public string[] sceneries;

    // Load and return a Configurations object from a .json file
    public static Configurations Load(string path)
    {
        if (System.IO.File.Exists(path))
        {
            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(path);
                Configurations c = null;
                try
                {
                    c = JsonUtility.FromJson<Configurations>(json);
                    return c;
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
        return null;
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