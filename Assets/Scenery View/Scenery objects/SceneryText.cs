using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class SceneryTextData : SceneryObjectData
{
    public float x, y, z;
    public float rotation;
    public string text;
    public int fontSize;
    public float red;
    public float green;
    public float blue;
    public float alpha;
}

public class SceneryText : MonoBehaviour, SceneryObject
{

    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        // Generate a SceneryTextData with current state's information
        
        SceneryTextData sceneryTextData = new SceneryTextData();
        sceneryTextData.x = transform.position.x;
        sceneryTextData.y = transform.position.y;
        sceneryTextData.z = transform.position.z;
        sceneryTextData.rotation = transform.rotation.eulerAngles.z;

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
        transform.position = new Vector3(sceneryTextData.x, sceneryTextData.y, sceneryTextData.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, sceneryTextData.rotation);

        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = sceneryTextData.text;
            textComponent.fontSize = sceneryTextData.fontSize;
            textComponent.color = new Color(sceneryTextData.red, sceneryTextData.green, sceneryTextData.blue, sceneryTextData.alpha);
        }
    }
}
