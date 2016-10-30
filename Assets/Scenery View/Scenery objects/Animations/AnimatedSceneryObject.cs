using UnityEngine;
using System.Collections;

public class AnimatedSceneryObject : MonoBehaviour {

    [Header("Horizontal Animation")]
    public float horizontalAnimationSpeed;
    public float horizontalAnimationMagnitude;

    [Header("Vertical Animation")]
    public float verticalAnimationSpeed;
    public float verticalAnimationMagnitude;

    Material mat;

    void Start () {
        // Get the material from the renderer
        mat = GetComponent<Renderer>().material;
        SetAnimation();

    }

    public void SetAnimation()
    {
        if (mat == null)
            return;

        // Set shader animation values for the material
        mat.SetFloat("_AnimationXSpeed", horizontalAnimationSpeed);
        mat.SetFloat("_AnimationYSpeed", verticalAnimationSpeed);

        mat.SetFloat("_AnimationXMagnitude", horizontalAnimationMagnitude);
        mat.SetFloat("_AnimationYMagnitude", verticalAnimationMagnitude);
    }

    public void OnValidate()
    {
        SetAnimation();
    }
}
