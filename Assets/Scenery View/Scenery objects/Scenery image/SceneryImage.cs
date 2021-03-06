﻿using UnityEngine;
using System.Collections;
using System.IO;

/*
SceneryImage is a scenery element that displays an image in the scenery.
*/

// The serialized data object for saving and loading the scenery image
[System.Serializable]
public class SceneryImageData : SceneryObjectData
{
    public MovableSceneryObjectData transform;
    public AnimatedSceneryObjectData animation;
    public string fileName = "";
    public bool restrictHorizontalMovement = false;
    public bool restrictVerticalMovement = false;
    public float transparencyRed = 0;
    public float transparencyGreen = 0;
    public float transparencyBlue = 0;
    public float transparencyThreshold = 0;
}


[RequireComponent(typeof(MovableSceneryObject))]
[RequireComponent(typeof(AnimatedSceneryObject))]
public class SceneryImage : MonoBehaviour, SceneryObject
{
    public string fileName;
    public bool restrictHorizontalMovement;
    public bool restrictVerticalMovement;
    public Color transparencyColor;
    public float transparencyThreshold;

    private bool imageLoaded;
    private IEnumerator loadingCoroutine;
    private float pixelsPerUnit = 100;

    void OnDestroy()
    {
        UnloadImage();
    }

    // Load an image from any local image file to a texture, create a sprite from it and assign that to this scenery image.
    public void LoadImage()
    {
        UnloadImage();

        loadingCoroutine = LoadImageInBackground();
        StartCoroutine(loadingCoroutine);
    }

    // Start loading the image asynchronously
    IEnumerator LoadImageInBackground()
    {
        imageLoaded = false;

        string absolutePath = fileName;
        
        // Get the path
        if (!fileName.StartsWith("http://") && !fileName.StartsWith("https://"))
        {
            absolutePath = "file:///" + GetAbsolutePath(fileName);
        }

        // Start the download
        WWW imageWWW = new WWW(absolutePath);

        //Wait for the file to finish loading
        while (!imageWWW.isDone)
        {
            yield return null;
        }

        // Test if the image is actually a supported movie
        if (fileName.Contains(".ogv"))
        {
            // Get the movie from the WWW
            MovieTexture movieTexture = imageWWW.movie;

            while (!movieTexture.isReadyToPlay)
            {
                yield return movieTexture;
            }

            // Create new sprite with the right dimensions
            Sprite sprite = Sprite.Create(new Texture2D(movieTexture.width, movieTexture.height), new Rect(0, 0, movieTexture.width, movieTexture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);

            // Create new MaterialPropertyBlock and assign the movieTexture to it
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture("_MainTex", movieTexture);

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();           
            spriteRenderer.sprite = sprite;

            // Give the movie to the renderer material
            spriteRenderer.SetPropertyBlock(block);

            // Set the movie to loop and play
            movieTexture.loop = true;
            movieTexture.Play();
        }
        else
        {
            // Load the image to a new texture
            Texture2D texture = new Texture2D(2, 2);
            imageWWW.LoadImageIntoTexture(texture);

            // Create a sprite from the new texture.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        // Now that the sprite is loaded, setup animation (which requires the sprite's information)
        GetComponent<AnimatedSceneryObject>().SetAnimation();

        // Set additional material properties such as transparency
        SetMaterialProperties();

        imageLoaded = true;
    }

    // Returns true if an image has been loaded
    public bool ImageLoaded()
    {
        return imageLoaded;
    }

    private void SetMaterialProperties()
    {
        Material mat = GetComponent<SpriteRenderer>().material;
        if (mat != null)
        {
            mat.SetColor("_TransparentColor", transparencyColor);
            mat.SetFloat("_Threshold", transparencyThreshold);
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

    // Returns an array (of four length) that contains the corners of the image in world space.
    public Vector3[] CornersInWorldSpace()
    {
        Vector3[] corners = new Vector3[4];
        int i = 0;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite sprite = spriteRenderer.sprite;
        if (sprite != null)
        {
            Vector3 leftTop = new Vector3(-sprite.pivot.x, sprite.pivot.y) / sprite.pixelsPerUnit;
            corners[i++] = transform.TransformPoint(leftTop);
            Vector3 leftBot = new Vector3(-sprite.pivot.x, -sprite.texture.height + sprite.pivot.y) / sprite.pixelsPerUnit;
            corners[i++] = transform.TransformPoint(leftBot);
            Vector3 rightTop = new Vector3(sprite.texture.width - sprite.pivot.x, sprite.pivot.y) / sprite.pixelsPerUnit;
            corners[i++] = transform.TransformPoint(rightTop);
            Vector3 rightBot = new Vector3(sprite.texture.width - sprite.pivot.x, -sprite.texture.height + sprite.pivot.y) / sprite.pixelsPerUnit;
            corners[i++] = transform.TransformPoint(rightBot);
        }

        return corners;
    }

    // Generates an upright rectangle that is contained within the image rectangle, rotated or not. Coordinates are in world space.
    public Rect MinimumUprightRectangle()
    {
        Vector3[] corners = CornersInWorldSpace();

        Vector2 center = Vector2.zero;
        foreach (Vector3 corner in corners)
        {
            center += (Vector2)corner;
        }
        center /= corners.Length;
        
        Rect rect = new Rect(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
        foreach (Vector3 corner in corners)
        {
            if (corner.x >= center.x && corner.x < rect.xMax)
            {
                rect.xMax = corner.x;
            }
            if (corner.x < center.x && corner.x > rect.x)
            {
                rect.x = corner.x;
            }
            if (corner.y >= center.y && corner.y < rect.yMax)
            {
                rect.yMax = corner.y;
            }
            if (corner.y < center.y && corner.y > rect.y)
            {
                rect.y = corner.y;
            }
        }

        return rect;
    }

    // SceneryObject interface methods
    public SceneryObjectData GetData()
    {
        // Generate a SceneryImageData with current state's information
        SceneryImageData sceneryImageData = new SceneryImageData();
        sceneryImageData.transform = (MovableSceneryObjectData)GetComponent<MovableSceneryObject>().GetData();
        sceneryImageData.animation = (AnimatedSceneryObjectData)GetComponent<AnimatedSceneryObject>().GetData();
        sceneryImageData.fileName = fileName;
        sceneryImageData.restrictHorizontalMovement = restrictHorizontalMovement;
        sceneryImageData.restrictVerticalMovement = restrictVerticalMovement;
        Material mat = GetComponent<SpriteRenderer>().material;
        Color col = mat.GetColor("_TransparentColor");
        sceneryImageData.transparencyRed = col.r;
        sceneryImageData.transparencyGreen = col.g;
        sceneryImageData.transparencyBlue = col.b;
        sceneryImageData.transparencyThreshold = mat.GetFloat("_Threshold");
        return sceneryImageData;
    }
    public void SetData(SceneryObjectData sceneryObjectData)
    {
        // Modify current state to match the given data's information
        SceneryImageData sceneryImageData = (SceneryImageData)sceneryObjectData;
        GetComponent<MovableSceneryObject>().SetData(sceneryImageData.transform);
        GetComponent<AnimatedSceneryObject>().SetData(sceneryImageData.animation);
        fileName = sceneryImageData.fileName;
        restrictHorizontalMovement = sceneryImageData.restrictHorizontalMovement;
        restrictVerticalMovement = sceneryImageData.restrictVerticalMovement;
        transparencyColor = new Color(sceneryImageData.transparencyRed, sceneryImageData.transparencyGreen, sceneryImageData.transparencyBlue, 1);
        transparencyThreshold = sceneryImageData.transparencyThreshold;

        LoadImage();
    }
}