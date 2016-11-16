using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[System.Serializable]
public class AnimatedSceneryObjectData : SceneryObjectData
{
    public float horizontalAnimationSpeed;
    public float horizontalAnimationMagnitude;
    public float verticalAnimationSpeed;
    public float verticalAnimationMagnitude;
}

public class AnimatedSceneryObject : MonoBehaviour, SceneryObject
{

    [Header("Horizontal Animation")]
    public float horizontalAnimationSpeed;
    public float horizontalAnimationMagnitude;

    [Header("Vertical Animation")]
    public float verticalAnimationSpeed;
    public float verticalAnimationMagnitude;

    Renderer[] renderers;
    Text[] textElements;

    void Start()
    {
        // Get renderers and text elements
        renderers = GetComponentsInChildren<Renderer>();
        textElements = GetComponentsInChildren<Text>();

        //SetAnimation();
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

                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    if (spriteRenderer.sprite != null)
                    {
                        mat.SetFloat("_HeightOffset", spriteRenderer.sprite.texture.height / spriteRenderer.sprite.pixelsPerUnit / 2.0f);
                    }
                }
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

    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        AnimatedSceneryObjectData data = new AnimatedSceneryObjectData();
        data.horizontalAnimationMagnitude = horizontalAnimationMagnitude;
        data.horizontalAnimationSpeed = horizontalAnimationSpeed;
        data.verticalAnimationMagnitude = verticalAnimationMagnitude;
        data.verticalAnimationSpeed = verticalAnimationSpeed;
        return data;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        AnimatedSceneryObjectData data = (AnimatedSceneryObjectData)sceneryObjectData;
        horizontalAnimationMagnitude = data.horizontalAnimationMagnitude;
        horizontalAnimationSpeed = data.horizontalAnimationSpeed;
        verticalAnimationMagnitude = data.verticalAnimationMagnitude;
        verticalAnimationSpeed = data.verticalAnimationSpeed;
        SetAnimation();
    }
}
