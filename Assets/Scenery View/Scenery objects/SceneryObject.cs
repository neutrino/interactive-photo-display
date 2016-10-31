using UnityEngine;
using System.Collections;

// A parent class for a SceneryObject implementer's internal data object
[System.Serializable]
public class SceneryObjectData {}

// All scenery objects that need serialization for loading and saving need to implement this interface
public interface SceneryObject
{
    SceneryObjectData GetData();
    void SetData(SceneryObjectData sceneryObjectData);
}
