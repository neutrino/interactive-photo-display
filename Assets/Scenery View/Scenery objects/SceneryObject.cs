using UnityEngine;
using System.Collections;

/*
SceneryObject is an interface that all scenery object components should implement if they
contain data that should be loaded or saved in the scenery JSON files.
*/

public interface SceneryObject
{
    // Generate a SceneryObjectData instance from the component's data
    SceneryObjectData GetData();
    // Initialize or modify the scenery object component according to the given SceneryObjectData
    void SetData(SceneryObjectData sceneryObjectData);
}


// A parent class for a SceneryObject implementer's internal data object. The class is serializable
// so that the JSON writing and reading methods can be used directly on the classes instances.
// Each component that uses this should implement a class that inherits SceneryObjectData.
[System.Serializable]
public class SceneryObjectData { }