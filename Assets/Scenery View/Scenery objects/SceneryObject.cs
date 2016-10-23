using UnityEngine;
using System.Collections;

// A parent class for a SceneryObject implementer's internal data object
[System.Serializable]
public class SceneryObjectData {}

// All scenery objects that need serialization for loading and saving need to implement this interface
public interface SceneryObject
{

    // Sync current state to the serializable data object
    void SyncToData();
    // Sync current state from existing data
    void SyncFromData();
    // Get the current data object
    SceneryObjectData GetData();
    // Set a new data object
    void SetData(SceneryObjectData newData);

}
