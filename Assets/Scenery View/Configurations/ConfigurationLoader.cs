using UnityEngine;
using System.Collections;

/*
ConfigurationLoader is a helper component used to load Configurations at startup.
*/

public class ConfigurationLoader : MonoBehaviour
{

    [Header("When running from the Unity editor, use this path to load configurations")]
    public string configurationsPath;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            // Load configurations from the path given as a command line argument
            Configurations.Load(args[1]);
        }
        else
        {
            Configurations.Load(configurationsPath);
        }
    }
}
