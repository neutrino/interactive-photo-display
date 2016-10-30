using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedSceneryObject : MonoBehaviour {

    [Header("Horizontal Animation")]
    public float horizontalAnimationSpeed;
    public float horizontalAnimationMagnitude;

    [Header("Vertical Animation")]
    public float verticalAnimationSpeed;
    public float verticalAnimationMagnitude;

    Renderer[] renderers;
    Text[] textElements;

    void Start () {
        // Get renderers and text elements
        renderers = GetComponentsInChildren<Renderer>();
        textElements = GetComponentsInChildren<Text>();
        SetAnimation();

    }

    public void SetAnimation()
    {
        if (renderers != null)
        {
            foreach (Renderer rend in renderers)
            {
                Material mat = rend.material;

                if (mat == null)
                    return;

                // Set shader animation values for the material
                mat.SetFloat("_AnimationXSpeed", horizontalAnimationSpeed);
                mat.SetFloat("_AnimationYSpeed", verticalAnimationSpeed);

                mat.SetFloat("_AnimationXMagnitude", horizontalAnimationMagnitude);
                mat.SetFloat("_AnimationYMagnitude", verticalAnimationMagnitude);
            }
        }

        if (textElements != null)
        {
            foreach (Text textElement in textElements)
            {
                // Create a new material for each text element
                Material mat = new Material(textElement.material);

                // Set shader animation values for the material
                mat.SetFloat("_AnimationXSpeed", horizontalAnimationSpeed);
                mat.SetFloat("_AnimationYSpeed", verticalAnimationSpeed);

                mat.SetFloat("_AnimationXMagnitude", horizontalAnimationMagnitude);
                mat.SetFloat("_AnimationYMagnitude", verticalAnimationMagnitude);

                textElement.material = mat;
            }
        }
    }

    public void OnValidate()
    {
        SetAnimation();
    }
}
