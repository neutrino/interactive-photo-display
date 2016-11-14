using UnityEngine;
using System.Collections;
using System;

public class SceneryQueue : MonoBehaviour
{

    public string configurationsPath;
    public GameObject sceneryPrefab;
    public Camera sceneryCamera;
    public float cameraPixelsPerUnits = 100.0f;

    private System.DateTime previousSceneryLoadTime;
    private int currentSceneryIndex;
    private GameObject currentSceneryObject = null;

    private Configurations configurations;
    private string[] sceneryQueue;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            // Load configurations from the path given as a command line argument
            configurations = Configurations.Load(args[1]);
        }
        else
        {
            // Load configurations from the path given in the editor.
            configurations = Configurations.Load(configurationsPath);
        }

        if (configurations != null)
        {
            BeginQueue(configurations);
        }
    }

    void Update()
    {
        if (configurations != null)
        {
            // Check the time since the last scenery change and advance in queue if enough time has passed
            if (configurations.sceneries.Length > 1)
            {
                DateTime currentTime = DateTime.Now;
                if (currentTime >= previousSceneryLoadTime.AddSeconds(configurations.sceneryChangeInterval))
                {
                    previousSceneryLoadTime = DateTime.Now;
                    NextScenery();
                }
            }
        }
    }

    // Begin the queue by initializing whatever's needed from the configurations and creating the first scenery in queue
    public void BeginQueue(Configurations configurations)
    {
        if (configurations != null && sceneryPrefab != null && sceneryCamera != null)
        {
            // Set the camera's size
            sceneryCamera.orthographicSize = configurations.cameraHeight / cameraPixelsPerUnits;
            // Initialize a copy of the scenery queue (a copy so that it can be shuffled if needed)
            sceneryQueue = new string[configurations.sceneries.Length];
            configurations.sceneries.CopyTo(sceneryQueue, 0);

            // Start the queue from the beginning
            currentSceneryIndex = -1;
            previousSceneryLoadTime = DateTime.Now;
            NextScenery();
        }
    }

    // Go to the next scenery in queue
    private void NextScenery()
    {
        // Start by destroying the current scenery
        if (currentSceneryObject != null)
        {
            DestroyImmediate(currentSceneryObject);
        }
        // Advance in the queue and shuffle it every time we start at 0
        currentSceneryIndex = ++currentSceneryIndex % configurations.sceneries.Length;
        if (currentSceneryIndex == 0 && configurations.shuffleSceneries)
        {
            sceneryQueue = ShuffledSceneries(sceneryQueue, new System.Random());
        }
        // Create the new scenery
        currentSceneryObject = CreateScenery(currentSceneryIndex);
    }

    // Create a scenery object that's in the queue at sceneryIndex position
    private GameObject CreateScenery(int sceneryIndex)
    {
        if (sceneryIndex >= 0 && sceneryIndex < sceneryQueue.Length)
        {
            GameObject sceneryObject = Instantiate(sceneryPrefab);
            Scenery scenery = sceneryObject.GetComponent<Scenery>();
            if (scenery != null)
            {
                scenery.SetConfigurations(configurations);
                scenery.LoadScenery(sceneryQueue[sceneryIndex]);
            }
            return sceneryObject;
        }
        return null;
    }

    // Return a shuffled array of sceneries from the original one.
    // This could be modified to be a generic array shuffle?
    private string[] ShuffledSceneries(string[] sceneries, System.Random random)
    {
        string[] shuffledSceneries = new string[sceneries.Length];
        sceneries.CopyTo(shuffledSceneries, 0);
        int n = sceneries.Length;
        while (n > 1)
        {
            int k = random.Next(n--);
            string tmp = shuffledSceneries[n];
            shuffledSceneries[n] = shuffledSceneries[k];
            shuffledSceneries[k] = tmp;
        }
        return shuffledSceneries;
    }
}
