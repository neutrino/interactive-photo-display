using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceView : MonoBehaviour
{
    public ColorSourceManager ColorSourceManager;
    [Range(0, 1)]
    public float alpha = 1;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        if (ColorSourceManager != null)
        {
            Texture texture = ColorSourceManager.GetColorTexture();
            Renderer renderer = gameObject.GetComponent<Renderer>();
            Color color = renderer.sharedMaterial.color;
            color.a = alpha;
            renderer.sharedMaterial.color = color;
            renderer.sharedMaterial.mainTexture = texture;

            Vector3 scale = transform.localScale;
            scale.x = -scale.y * ((float)texture.width / (float)texture.height);
            transform.localScale = scale;
        }
    }
}
