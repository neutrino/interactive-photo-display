using UnityEngine;
using System.Collections;

/*
ApplicationQuitter makes it possible to quit the application with a key press.
*/

public class ApplicationQuitter : MonoBehaviour
{

    public KeyCode quitKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(quitKey))
        {
            Application.Quit();
        }
    }

}
