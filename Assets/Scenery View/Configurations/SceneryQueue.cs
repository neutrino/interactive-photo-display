using UnityEngine;
using System.Collections;
using System;

public class SceneryQueue : MonoBehaviour
{
    public GameObject sceneryPrefab;
    public GameObject sceneryTransitionPrefab;
    public KeyCode sceneryChangeKey = KeyCode.Space;

    private System.DateTime previousSceneryLoadTime;
    private int currentSceneryIndex;
    private GameObject currentSceneryObject = null;
    private bool currentlyChangingScenery = false;

    private Configurations configurations;
    private string[] sceneryQueue;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            // Load configurations from the path given as a command line argument
            Configurations configs = Configurations.Load(args[1]);

            if (configs != null)
            {
                BeginQueue(configs);
            }
        }
    }

    void Update()
    {
        if (configurations != null)
        {
            if (configurations.sceneries.Length > 1)
            {
                // Advance in queue if enough time has passed
                if (configurations.sceneryChangeInterval > 0)
                {
                    DateTime currentTime = DateTime.Now;
                    if (currentTime >= previousSceneryLoadTime.AddSeconds(configurations.sceneryChangeInterval))
                    {
                        previousSceneryLoadTime = currentTime;
                        StartCoroutine("NextScenery");
                    }
                }
                // Advance in queue via keyboard input
                if (Input.GetKeyDown(sceneryChangeKey))
                {
                    previousSceneryLoadTime = DateTime.Now;
                    StartCoroutine("NextScenery");
                }
            }
        }
    }

    // Begin the queue by initializing whatever's needed from the configurations and creating the first scenery in queue
    public void BeginQueue(Configurations configurations)
    {
        this.configurations = configurations;
        if (configurations != null && sceneryPrefab != null)
        {
            // Initialize a copy of the scenery queue (a copy so that it can be shuffled if needed)
            sceneryQueue = new string[configurations.sceneries.Length];
            configurations.sceneries.CopyTo(sceneryQueue, 0);

            // Start the queue from the beginning
            currentSceneryIndex = -1;
            previousSceneryLoadTime = DateTime.Now;
            StartCoroutine("NextScenery");
        }
    }

    // Go to the next scenery in queue
    IEnumerator NextScenery()
    {
        if (!currentlyChangingScenery)
        {
            currentlyChangingScenery = true;

            SceneryTransition sceneryTransition = null;
            if (sceneryTransitionPrefab != null)
            {
                sceneryTransition = Instantiate(sceneryTransitionPrefab).GetComponent<SceneryTransition>();
            }
            if (sceneryTransition != null)
            {
                sceneryTransition.color = new Color(configurations.transitionColorRed, configurations.transitionColorGreen, configurations.transitionColorBlue);
                sceneryTransition.duration = configurations.transitionDuration;
                while (!sceneryTransition.FadedIn())
                {
                    yield return null;
                }
            }

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

            if (sceneryTransition != null)
            {
                sceneryTransition.FadeOut();
            }

            currentlyChangingScenery = false;
        }
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
