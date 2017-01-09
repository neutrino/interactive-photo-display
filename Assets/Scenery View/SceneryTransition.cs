using UnityEngine;
using System.Collections;

public class SceneryTransition : MonoBehaviour
{

    public Texture texture;

    [HideInInspector]
    public float duration = 3;
    [HideInInspector]
    public Color color = Color.black;

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
            if (timer < duration)
            {
                timer += Time.deltaTime;
                alpha = timer / duration;
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
                alpha = timer / duration;
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
        if (texture != null && alpha > 0)
        {
            GUI.depth = -1;
            GUI.color = new Color(color.r, color.g, color.b, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
        }
    }

}
