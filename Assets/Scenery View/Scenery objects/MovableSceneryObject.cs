﻿using UnityEngine;
using System.Collections;

/*
MovableSceneryObject is a component used by scenery elements to enable movement with the scenery.
*/

[System.Serializable]
public class MovableSceneryObjectData : SceneryObjectData
{
    public Vector3 position = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public float rotation = 0;
}

public class MovableSceneryObject : MonoBehaviour, SceneryObject
{

    // Starting transform values used for relative transforming after initialization.
    private Vector3 startingPosition;
    private Vector3 startingScale;

    void Start()
    {
        // Initialize starting transform values
        startingPosition = transform.localPosition;
        startingScale = transform.localScale;
    }

    // Move the transform to a position relative to the starting position.
    public void SetRelativePosition(Vector3 position)
    {
        transform.localPosition = startingPosition + position;
    }

    // Scale the transform to a scale relative to the starting scale.
    public void SetRelativeScale(Vector3 scale)
    {
        Vector3 newScale = startingScale;
        newScale.x *= scale.x;
        newScale.y *= scale.y;
        newScale.z *= scale.z;
        transform.localScale = newScale;
    }

    // Set the base scale
    public void SetBaseScale(Vector3 scale)
    {
        startingScale = scale;
    }

    public Vector3 GetRelativePosition()
    {
        return transform.localPosition - startingPosition;
    }
    public Vector3 GetRelativeScale()
    {
        Vector3 scale = transform.localScale;
        scale.x /= startingScale.x;
        scale.y /= startingScale.y;
        scale.z /= startingScale.z;
        return scale;
    }


    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        MovableSceneryObjectData data = new MovableSceneryObjectData();
        data.position = transform.position;
        data.scale = transform.localScale;
        data.rotation = transform.rotation.eulerAngles.z;
        return data;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        MovableSceneryObjectData data = (MovableSceneryObjectData)sceneryObjectData;
        transform.position = data.position;
        transform.localScale = data.scale;
        startingPosition = data.position;
        startingScale = data.scale;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, data.rotation);
    }

}
