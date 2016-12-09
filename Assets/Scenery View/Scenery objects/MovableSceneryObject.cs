using UnityEngine;
using System.Collections;

[System.Serializable]
public class MovableSceneryObjectData : SceneryObjectData
{
    public float x, y, z;
    public float rotation;
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
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.rotation = transform.rotation.eulerAngles.z;
        return data;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        MovableSceneryObjectData data = (MovableSceneryObjectData)sceneryObjectData;
        transform.position = new Vector3(data.x, data.y, data.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, data.rotation);
    }

}
