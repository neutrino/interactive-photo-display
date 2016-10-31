using UnityEngine;
using System.Collections;
using System.IO;


// The serialized data object for saving and loading the scenery image
[System.Serializable]
public class SceneryImageData : SceneryObjectData
{
    public MovableSceneryObjectData movableSceneryObjectData;
    public AnimatedSceneryObjectData animatedSceneryObjectData;
    public string fileName;
}


[RequireComponent(typeof(MovableSceneryObject))]
[RequireComponent(typeof(AnimatedSceneryObject))]
public class SceneryImage : MonoBehaviour, SceneryObject
{
    public string fileName;
    
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
    public SceneryObjectData GetData()
    {
        // Generate a SceneryImageData with current state's information
        SceneryImageData sceneryImageData = new SceneryImageData();
        sceneryImageData.movableSceneryObjectData = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryImageData.animatedSceneryObjectData = (AnimatedSceneryObjectData)GetComponent<AnimatedSceneryObject>().GetData();
        sceneryImageData.fileName = fileName;
        return sceneryImageData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryImageData sceneryImageData = (SceneryImageData)sceneryObjectData;
        GetComponent<MovableSceneryObject>().SetData(sceneryImageData.movableSceneryObjectData);
        GetComponent<AnimatedSceneryObject>().SetData(sceneryImageData.animatedSceneryObjectData);
        fileName = sceneryImageData.fileName;
        LoadImage();
    }
}