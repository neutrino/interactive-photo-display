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
