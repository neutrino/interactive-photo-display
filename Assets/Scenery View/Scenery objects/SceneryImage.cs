using UnityEngine;
using System.Collections;
using System.IO;


// The serialized data object for saving and loading the scenery image
[System.Serializable]
public class SceneryImageData : SceneryObjectData
{
    public float x, y, z;
    public float rotation;
    public string fileName;
}



public class SceneryImage : MonoBehaviour, SceneryObject
{
    public string fileName;

    private SceneryImageData data = new SceneryImageData();
    
    void OnDestroy()
    {
        UnloadImage();
    }

    // Load an image from any local image file to a texture, create a sprite from it and assign that to this scenery image.
    public void LoadImage()
    {
        string absolutePath = GetAbsolutePath(fileName);

        if (File.Exists(absolutePath))
        {
            // First unload any previously loaded image.
            UnloadImage();

            // Read the bytes from the file to a new texture.
            byte[] imageData = File.ReadAllBytes(absolutePath);
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(imageData))
            {
                Debug.Log("Cannot load image from '" + absolutePath + "'.");
            }

            // Create a sprite from the new texture.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
        else
        {
            Debug.Log("File at '" + absolutePath + "' doesn't exist.");
        }
    }

    // Unload and unassign the previously loaded image. Free all memory that was allocated for the image.
    public void UnloadImage()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite != null)
        {
            DestroyImmediate(spriteRenderer.sprite.texture);
            DestroyImmediate(spriteRenderer.sprite);
            spriteRenderer.sprite = null;
        }
    }


    private string GetAbsolutePath(string fileName)
    {
        // If the file exists, it's already an absolute path
        if (File.Exists(fileName))
        {
            return fileName;
        }
        else
        {
            // Find the file's directory from the parent (Scenery)
            Transform parent = transform.parent;
            if (parent != null)
            {
                Scenery scenery = parent.GetComponent<Scenery>();
                if (scenery != null)
                {
                    return System.IO.Path.GetDirectoryName(scenery.FilePath()) + "\\" + fileName;
                }
            }
        }
        return "";
    }


    // SceneryObject interface methods
    public void SyncToData()
    {
        // Sync current state to the data object
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.rotation = transform.rotation.eulerAngles.z;
        data.fileName = fileName;
    }
    public void SyncFromData()
    {
        // Sync state from the existing data object
        transform.position = new Vector3(data.x, data.y, data.z);
        transform.rotation = Quaternion.Euler(0, 0, data.rotation);
        fileName = data.fileName;
        LoadImage();
    }
    public SceneryObjectData GetData()
    {
        return data;
    }
    public void SetData(SceneryObjectData newData)
    {
        data = (SceneryImageData)newData;
    }
}