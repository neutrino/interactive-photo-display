﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class SceneryPopUpData : SceneryObjectData
{
    public MovableSceneryObjectData transform;
    public string text = "";
    public int fontSize = 8;
    public float width = 100;

    public Color textColor = Color.white;
    public Color backgroundColor = Color.clear;

    public bool alwaysVisible;
    public bool alwaysOnTop;

}

[RequireComponent(typeof(MovableSceneryObject))]
public class SceneryPopUp : MonoBehaviour, SceneryObject
{
    
    public class PopUpShownEventArgs : System.EventArgs
    {
    }
    public class PopUpHiddenEventArgs : System.EventArgs
    {
    }

    public delegate void PopUpShownDelegate(object sceneryPopUp, PopUpShownEventArgs popUpInfo);
    public static event PopUpShownDelegate PopUpShown;
    public delegate void PopUpHiddenDelegate(object sceneryPopUp, PopUpHiddenEventArgs popUpInfo);
    public static event PopUpHiddenDelegate PopUpHidden;

    public bool alwaysVisible;
    public float targetScale = 0f;
    public float animationSpeed = 1f;
    public float handOnIconMaxDistance = 50.0f;

    public Texture iconTexture;

    private bool visible;

    private MovableSceneryObject movableSceneryObject;
    private Image backgroundImage;
    private RectTransform backgroundImageTransform;

    void Awake()
    {
        movableSceneryObject = GetComponent<MovableSceneryObject>();
        backgroundImage = GetComponentInChildren<Image>();
        if (backgroundImage != null)
        {
            backgroundImageTransform = backgroundImage.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        // Smooth scaling to target scale
        if (backgroundImageTransform != null)
        {
            Vector3 scale = backgroundImageTransform.localScale;
            scale.y += (targetScale - scale.y) * Time.deltaTime * animationSpeed;
            backgroundImageTransform.localScale = scale;
        }
    }

    public void ToggleVisibility()
    {
        SetVisibility(!visible);
    }

    public void SetVisibility(bool isVisible)
    {
        if (!alwaysVisible)
        {
            if (isVisible)
            {
                visible = true;
                targetScale = 1;
                if (PopUpShown != null)
                {
                    PopUpShown(this, new PopUpShownEventArgs());
                }
            }
            else
            {
                visible = false;
                targetScale = 0;
                if (PopUpHidden != null)
                {
                    PopUpHidden(this, new PopUpHiddenEventArgs());
                }
            }
        }
    }

    public bool ScreenPointCloseEnough(Vector2 pointOnScreen)
    {
        if (visible)
        {
            Rect rect = RectangleOnScreen();
            return pointOnScreen.x >= rect.xMin && pointOnScreen.x <= rect.xMax && pointOnScreen.x >= rect.yMin && pointOnScreen.y <= rect.yMax;
        }
        else
        {
            Vector2 myPositionOnScreen = RectangleOnScreen().center;
            return Vector2.Distance(pointOnScreen, myPositionOnScreen) < handOnIconMaxDistance;
        }
    }
    
    public Rect RectangleOnScreen()
    {
        Rect rect = new Rect();
        if (backgroundImageTransform != null)
        {
            Rect transformRect = backgroundImageTransform.rect;
            Camera camera = Camera.main;
            Vector2 leftTop = camera.WorldToScreenPoint(backgroundImageTransform.TransformPoint(transformRect.xMin, transformRect.yMax, 0));
            Vector2 rightBot = camera.WorldToScreenPoint(backgroundImageTransform.TransformPoint(transformRect.xMax, transformRect.yMin, 0));
            rect.xMin = leftTop.x;
            rect.xMax = rightBot.x;
            rect.yMin = Screen.height - leftTop.y;
            rect.yMax = Screen.height - rightBot.y;
        }

        return rect;
    }

    public string GetText()
    {
        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            return textComponent.text;
        }
        return "";
    }

    
    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        // Generate a SceneryPopUpData with current state's information

        SceneryPopUpData sceneryPopUpData = new SceneryPopUpData();

        sceneryPopUpData.transform = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryPopUpData.alwaysVisible = alwaysVisible;
        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            sceneryPopUpData.text = textComponent.text;
            sceneryPopUpData.fontSize = textComponent.fontSize;
            sceneryPopUpData.textColor = textComponent.color;
        }

        if (backgroundImage != null)
        {
            sceneryPopUpData.backgroundColor = backgroundImage.color;

            sceneryPopUpData.width = backgroundImageTransform.sizeDelta.x;
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

        GetComponent<MovableSceneryObject>().SetData(sceneryPopUpData.transform);

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
            textComponent.color = sceneryPopUpData.textColor;
        }

        if (backgroundImage != null)
        {
            Color color = sceneryPopUpData.backgroundColor;
            // For some reason if the background's alpha is zero, the text in it is invisible.
            // Force alpha to be just above zero so that the text component is always visible.
            color.a = Mathf.Max(color.a, (1f / 255f) * 2f);
            backgroundImage.color = color;

            backgroundImageTransform.sizeDelta = new Vector2(sceneryPopUpData.width, 0f);
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

    void OnGUI()
    {
        if (iconTexture != null && !visible)
        {
            Vector2 center = RectangleOnScreen().center;
            Vector2 textureSize = new Vector2(iconTexture.width, iconTexture.height) * 0.5f;
            Vector2 point = new Vector2(center.x - textureSize.x / 2.0f, center.y - textureSize.y / 2.0f);
            
            GUI.DrawTexture(new Rect(point, textureSize), iconTexture);
        }
    }
}