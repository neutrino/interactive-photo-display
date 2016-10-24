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
}

public class SceneryText : MonoBehaviour, SceneryObject
{

    void SetText(string text)
    {
        Text textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
    string GetText()
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
        // Generate a SceneryTextData with current state's information
        SceneryTextData sceneryTextData = new SceneryTextData();
        sceneryTextData.x = transform.position.x;
        sceneryTextData.y = transform.position.y;
        sceneryTextData.z = transform.position.z;
        sceneryTextData.rotation = transform.rotation.eulerAngles.z;
        sceneryTextData.text = GetText();
        return sceneryTextData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryTextData sceneryTextData = (SceneryTextData)sceneryObjectData;
        transform.position = new Vector3(sceneryTextData.x, sceneryTextData.y, sceneryTextData.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, sceneryTextData.rotation);
        SetText(sceneryTextData.text);
    }
}
