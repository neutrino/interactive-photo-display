using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class SceneryTextData : SceneryObjectData
{
    public MovableSceneryObjectData transform;
    public AnimatedSceneryObjectData animation;
    public string text = "";
    public string font = "";
    public int fontSize = 8;
    public Color color = Color.white;
}

[RequireComponent(typeof(MovableSceneryObject))]
[RequireComponent(typeof(AnimatedSceneryObject))]
public class SceneryText : MonoBehaviour, SceneryObject
{

    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        // Generate a SceneryTextData with current state's information
        
        SceneryTextData sceneryTextData = new SceneryTextData();

        sceneryTextData.transform = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryTextData.animation = (AnimatedSceneryObjectData)GetComponent<AnimatedSceneryObject>().GetData();

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            sceneryTextData.text = textComponent.text;
            sceneryTextData.fontSize = textComponent.fontSize;
            sceneryTextData.color = textComponent.color;
        }

        return sceneryTextData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryTextData sceneryTextData = (SceneryTextData)sceneryObjectData;

        GetComponent<MovableSceneryObject>().SetData(sceneryTextData.transform);
        GetComponent<AnimatedSceneryObject>().SetData(sceneryTextData.animation);

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            if (sceneryTextData.font != "")
            {
                textComponent.font = Font.CreateDynamicFontFromOSFont(sceneryTextData.font, sceneryTextData.fontSize);
            }
            textComponent.text = sceneryTextData.text;
            textComponent.fontSize = sceneryTextData.fontSize;
            textComponent.color = sceneryTextData.color;
        }
    }
}
