using UnityEngine;
using System.Collections;
using System;

public class SceneryQueue : MonoBehaviour
{

    public class SceneryChangedEventArgs : System.EventArgs
    {
        public int newSceneryNumber;
        public string newSceneryPath;
        public SceneryChangedEventArgs(int newSceneryNumber, string newSceneryPath)
        {
            this.newSceneryNumber = newSceneryNumber;
            this.newSceneryPath = newSceneryPath;
        }
    }

    public delegate void SceneryChangedDelegate(object sceneryQueue, SceneryChangedEventArgs sceneryChangedInfo);
    public event SceneryChangedDelegate SceneryChanged;


    public GameObject sceneryPrefab;
    public GameObject sceneryTransitionPrefab;
    public KeyCode sceneryChangeKey = KeyCode.Space;

    private System.DateTime previousSceneryLoadTime;
    private int currentSceneryIndex;
    private GameObject currentSceneryObject = null;
    private bool currentlyChangingScenery = false;
    
    private string[] sceneryQueue;

    private BodyTracker bodyTracker;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length >= 2)
        {
            // Load configurations from the path given as a command line argument
            if (Configurations.Load(args[1]) != null)
            {
                BeginQueue();
            }
        }

        bodyTracker = FindObjectOfType<BodyTracker>();
    }

    void Update()
    {
        Configurations configs = Configurations.Instance();
        if (configs != null)
        {
            if (configs.sceneries.Length > 1)
            {
                // Advance in queue if enough time has passed
                if (configs.sceneryChangeInterval > 0)
                {
                    DateTime currentTime = DateTime.Now;
                    if (currentTime >= previousSceneryLoadTime.AddSeconds(configs.sceneryChangeInterval))
                    {
                        previousSceneryLoadTime = currentTime;
                        NextScenery();
                    }
                }
                // Advance in queue via keyboard input
                if (Input.GetKeyDown(sceneryChangeKey))
                {
                    previousSceneryLoadTime = DateTime.Now;
                    NextScenery();
                }
            }
        }
    }

    // Begin the queue by initializing whatever's needed from the configurations and creating the first scenery in queue
    public void BeginQueue()
    {
        Configurations configs = Configurations.Instance();
        if (configs != null && sceneryPrefab != null)
        {
            // Initialize a copy of the scenery queue (a copy so that it can be shuffled if needed)
            sceneryQueue = new string[configs.sceneries.Length];
            configs.sceneries.CopyTo(sceneryQueue, 0);

            // Start the queue from the beginning
            currentSceneryIndex = -1;
            previousSceneryLoadTime = DateTime.Now;
            NextScenery();
        }
    }

    public IEnumerator ChangeScenery(int sceneryIndex)
    {
        Configurations configs = Configurations.Instance();

        // Check for scenery index validity
        bool validSceneryIndex = false;
        if (configs != null)
        {
            if (sceneryIndex >= 0 && sceneryIndex < configs.sceneries.Length)
            {
                currentSceneryIndex = sceneryIndex;
                validSceneryIndex = true;
            }
        }

        if (!currentlyChangingScenery && validSceneryIndex)
        {
            currentlyChangingScenery = true;

            // Start the scenery transition and wait for its completion until moving on to the new scenery
            SceneryTransition sceneryTransition = null;
            if (sceneryTransitionPrefab != null)
            {
                sceneryTransition = Instantiate(sceneryTransitionPrefab).GetComponent<SceneryTransition>();
            }
            if (sceneryTransition != null)
            {
                if (configs != null)
                {
                    sceneryTransition.color = configs.transitionColor;
                    sceneryTransition.duration = configs.transitionDuration;
                }
                else
                {
                    sceneryTransition.color = Color.black;
                    sceneryTransition.duration = 1;
                }
                // Hold the coroutine until the transition is ready
                while (!sceneryTransition.FadedIn())
                {
                    yield return null;
                }
            }

            // Destroy the current scenery and create the new one
            if (currentSceneryObject != null)
            {
                DestroyImmediate(currentSceneryObject);
            }
            currentSceneryObject = CreateScenery(currentSceneryIndex);


            if (sceneryTransition != null)
            {
                sceneryTransition.FadeOut();
            }

            currentlyChangingScenery = false;
        }
    }

    // Go to the next scenery in queue
    public void NextScenery()
    {
        Configurations configs = Configurations.Instance();

        // Advance in the queue and shuffle it every time we start at 0
        int index = 0;
        bool shuffle = false;
        if (configs != null)
        {
            index = ++currentSceneryIndex % configs.sceneries.Length;
            shuffle = configs.shuffleSceneries;
        }
        currentSceneryIndex = index;
        if (currentSceneryIndex == 0 && shuffle)
        {
            sceneryQueue = ShuffledSceneries(sceneryQueue, new System.Random());
        }

        StartCoroutine(ChangeScenery(currentSceneryIndex));
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
                scenery.LoadScenery(sceneryQueue[sceneryIndex]);
                
                // Call the event for changing scenery
                if (SceneryChanged != null)
                {
                    SceneryChanged(this, new SceneryChangedEventArgs(sceneryIndex, sceneryQueue[sceneryIndex]));
                }
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
