using UnityEngine;
using System.Collections;
using System.IO;

[System.Serializable]
public class SceneryImageData
{
    public float x, y, z;
    public float rotation;
    public string fileName;
}

public class SceneryImage : MonoBehaviour
{
    public string fileName;

    public SceneryImageData data = new SceneryImageData();

    // Starting transform values used for relative transforming after initialization.
    Vector3 startingPosition;
    Vector3 startingScale;

    void Start()
    {
        // Initialize starting transform values
        startingPosition = transform.localPosition;
        startingScale = transform.localScale;

        // If we're running outside the Unity editor, load the image (because it might not have been loaded from the editor before building).
        if (!Application.isEditor)
        {
            LoadImage();
        }
    }

    // Load an image from any local image file to a texture, create a sprite from it and assign that to this scenery image.
    public void LoadImage()
    {
        string path = "";
        Transform parent = transform.parent;
        if (parent != null)
        {
            Scenery scenery = parent.GetComponent<Scenery>();
            if (scenery != null)
            {
                path = System.IO.Path.GetDirectoryName(scenery.FilePath()) + "\\" + fileName;
            }
        }
        if (File.Exists(path))
        {
            // First unload any previously loaded image.
            UnloadImage();

            // Read the bytes from the file to a new texture.
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            // Create a sprite from the new texture.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GetComponent<SpriteRenderer>().sprite = sprite;
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

    public void UpdateData()
    {
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.rotation = transform.rotation.eulerAngles.z;
        data.fileName = fileName;
    }
    public void UpdateFromData()
    {
        transform.position = new Vector3(data.x, data.y, data.z);
        transform.rotation = Quaternion.Euler(0, 0, data.rotation);
        fileName = data.fileName;
        LoadImage();
    }
}