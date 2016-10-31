using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class SceneryTextData : SceneryObjectData
{
    public MovableSceneryObjectData movableSceneryObjectData;
    public AnimatedSceneryObjectData animatedSceneryObjectData;
    public string text;
    public int fontSize;
    public float red;
    public float green;
    public float blue;
    public float alpha;
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

        sceneryTextData.movableSceneryObjectData = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryTextData.animatedSceneryObjectData = (AnimatedSceneryObjectData)GetComponent<AnimatedSceneryObject>().GetData();

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            sceneryTextData.text = textComponent.text;
            sceneryTextData.fontSize = textComponent.fontSize;
            sceneryTextData.red = textComponent.color.r;
            sceneryTextData.green = textComponent.color.g;
            sceneryTextData.blue = textComponent.color.b;
            sceneryTextData.alpha = textComponent.color.a;
        }

        return sceneryTextData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryTextData sceneryTextData = (SceneryTextData)sceneryObjectData;

        GetComponent<MovableSceneryObject>().SetData(sceneryTextData.movableSceneryObjectData);
        GetComponent<AnimatedSceneryObject>().SetData(sceneryTextData.animatedSceneryObjectData);

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = sceneryTextData.text;
            textComponent.fontSize = sceneryTextData.fontSize;
            textComponent.color = new Color(sceneryTextData.red, sceneryTextData.green, sceneryTextData.blue, sceneryTextData.alpha);
        }
    }
}
