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
    // Use this for initialization
    void Start () {
        mat = GetComponent<Renderer>().material;
        SetAnimation();

    }
	
	// Update is called once per frame
	void Update () {

        
	}

    public void SetAnimation()
    {
        if (mat == null)
            return;

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
