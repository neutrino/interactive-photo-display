using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class SceneryPopUpData : SceneryObjectData
{
    public MovableSceneryObjectData movableSceneryObjectData;
    public string text;
    public int fontSize;
    public float width;

    public float textRed;
    public float textGreen;
    public float textBlue;
    public float textAlpha;

    public float backgroundRed;
    public float backgroundGreen;
    public float backgroundBlue;
    public float backgroundAlpha;

    public bool alwaysVisible;
    public bool alwaysOnTop;

}

[RequireComponent(typeof(MovableSceneryObject))]
public class SceneryPopUp : MonoBehaviour, SceneryObject
{
    public bool visible;
    public bool alwaysVisible;
    public float targetScale = 0f;
    public float animationSpeed = 1f;

    private MovableSceneryObject movableSceneryObject;

    void Start()
    {
        movableSceneryObject = GetComponent<MovableSceneryObject>();
    }

    void Update()
    {
        // Smooth scaling to target scale
        movableSceneryObject.SetBaseScale(new Vector3(1, Mathf.Lerp(transform.localScale.y, targetScale, Time.deltaTime * animationSpeed), 1));

        // Shortcut key for toggling visibility
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SetVisibility(!visible);
        }
    }

    public void SetVisibility(bool isVisible)
    {
        if (!alwaysVisible)
        {
            if (isVisible)
            {
                visible = true;
                targetScale = 1;
            }
            else
            {
                visible = false;
                targetScale = 0;
            }
        }
    }

    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        // Generate a SceneryPopUpData with current state's information

        SceneryPopUpData sceneryPopUpData = new SceneryPopUpData();

        sceneryPopUpData.movableSceneryObjectData = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryPopUpData.alwaysVisible = alwaysVisible;
        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            sceneryPopUpData.text = textComponent.text;
            sceneryPopUpData.fontSize = textComponent.fontSize;
            sceneryPopUpData.textRed = textComponent.color.r;
            sceneryPopUpData.textGreen = textComponent.color.g;
            sceneryPopUpData.textBlue = textComponent.color.b;
            sceneryPopUpData.textAlpha = textComponent.color.a;
        }

        Image backgroundImage = GetComponentInChildren<Image>();

        if (backgroundImage != null)
        {
            sceneryPopUpData.backgroundRed = backgroundImage.color.r;
            sceneryPopUpData.backgroundGreen = backgroundImage.color.g;
            sceneryPopUpData.backgroundBlue = backgroundImage.color.b;
            sceneryPopUpData.backgroundAlpha = backgroundImage.color.a;

            RectTransform backgroundTransform = backgroundImage.GetComponent<RectTransform>();

            sceneryPopUpData.width = backgroundTransform.sizeDelta.x;
        }

        Canvas canvasComponent = GetComponentInChildren<Canvas>();

        if (canvasComponent != null)
        {
            if (canvasComponent.sortingOrder <= 0)
            {
                sceneryPopUpData.alwaysOnTop = false;
            }
            else
            {
                sceneryPopUpData.alwaysOnTop = true;
            }
        }



        return sceneryPopUpData;
    }

    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryPopUpData sceneryPopUpData = (SceneryPopUpData)sceneryObjectData;

        GetComponent<MovableSceneryObject>().SetData(sceneryPopUpData.movableSceneryObjectData);

        alwaysVisible = sceneryPopUpData.alwaysVisible;
        visible = alwaysVisible;
        if (alwaysVisible)
        {
            targetScale = 1;
        }

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = sceneryPopUpData.text;
            textComponent.fontSize = sceneryPopUpData.fontSize;
            textComponent.color = new Color(sceneryPopUpData.textRed, sceneryPopUpData.textGreen, sceneryPopUpData.textBlue, sceneryPopUpData.textAlpha);
        }

        Image backgroundImage = GetComponentInChildren<Image>();

        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(sceneryPopUpData.backgroundRed, sceneryPopUpData.backgroundGreen, sceneryPopUpData.backgroundBlue, sceneryPopUpData.backgroundAlpha);

            RectTransform backgroundTransform = backgroundImage.GetComponent<RectTransform>();

            backgroundTransform.sizeDelta = new Vector2(sceneryPopUpData.width, 0f);
        }

        Canvas canvasComponent = GetComponentInChildren<Canvas>();

        if (canvasComponent != null)
        {
            if (sceneryPopUpData.alwaysOnTop)
            {
                canvasComponent.sortingOrder = 1;
            }
            else
            {
                canvasComponent.sortingOrder = 0;
            }
        }
    }
}
