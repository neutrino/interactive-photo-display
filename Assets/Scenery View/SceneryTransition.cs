using UnityEngine;
using System.Collections;

public class SceneryTransition : MonoBehaviour
{

    public Texture blackTexture;
    public float fadeTime = 1;

    private float timer = 0;
    private float fadingDirection = 1;
    private float alpha = 0;

    private bool fadedIn = false;

    void Start()
    {
        FadeIn();
    }

    void Update()
    {

        if (fadingDirection > 0)
        {
            if (timer < fadeTime)
            {
                timer += Time.deltaTime;
                alpha = timer / fadeTime;
            }
            else
            {
                fadedIn = true;
            }
        }
        if (fadingDirection < 0)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                alpha = timer / fadeTime;
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }


    public void FadeIn()
    {
        fadingDirection = 1;
    }
    public void FadeOut()
    {
        fadingDirection = -1;
    }
    public bool FadedIn()
    {
        return fadedIn;
    }


    void OnGUI()
    {
        if (blackTexture != null && alpha > 0)
        {
            GUI.color = new Color(0, 0, 0, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
    }

}
