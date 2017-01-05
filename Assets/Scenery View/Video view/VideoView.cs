using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

public class VideoView : MonoBehaviour
{

    [Range(0, 1)]
    public float opacity = 1;

    private Kinect.KinectSensor kinectSensor;
    private Kinect.ColorFrameReader colorFrameReader;
    private Texture2D texture;
    private byte[] textureData;
    
    new private Renderer renderer;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        Configurations.Loaded += ConfigurationsLoaded;
    }

    void Start()
    {
        Enable(false);
        SetOpacity(opacity);

        StartKinect();

        if (kinectSensor != null)
        {
            // Initialize the texture
            var frameDesc = kinectSensor.ColorFrameSource.CreateFrameDescription(Kinect.ColorImageFormat.Rgba);
            texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            textureData = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
        }
    }

    void Update()
    {
        if (Enabled())
        {
            Vector3 scale = Vector3.one;
            if ((float)Camera.main.pixelWidth / (float)Camera.main.pixelHeight > (float)texture.width / (float)texture.height)
            {
                scale.y = Camera.main.orthographicSize * 2f;
                scale.x = -scale.y * ((float)texture.width / (float)texture.height);
            }
            else
            {
                scale.x = -(Camera.main.orthographicSize * 2f) * ((float)Camera.main.pixelWidth / (float)Camera.main.pixelHeight);
                scale.y = -scale.x * ((float)texture.height / (float)texture.width);
            }
            transform.localScale = scale;
        }
    }

    void OnDestroy()
    {
        CloseKinect();
    }

    public void Enable(bool enable)
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = enable;
        }
    }
    public bool Enabled()
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer.enabled;
        }
        return false;
    }

    private void SetOpacity(float opacity)
    {
        Color color = renderer.sharedMaterial.color;
        color.a = opacity;
        renderer.sharedMaterial.color = color;
    }

    private void ConfigurationsLoaded(object configurations, Configurations.LoadedEventArgs loadedInfo)
    {
        Configurations configs = (Configurations)configurations;
        Enable(configs.displayCameraFeed);
        SetOpacity(configs.cameraFeedAlpha);
    }

    // Open the sensor and its color frame reader
    void StartKinect()
    {
        kinectSensor = Kinect.KinectSensor.GetDefault();
        if (kinectSensor != null)
        {
            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
            }
            colorFrameReader = kinectSensor.ColorFrameSource.OpenReader();

            // Add a handler for when a frame arrives from the reader
            colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
        }
    }
    // Close the sensor and its color frame reader
    private void CloseKinect()
    {
        if (colorFrameReader != null)
        {
            colorFrameReader.Dispose();
            colorFrameReader = null;
        }

        if (kinectSensor != null)
        {
            if (kinectSensor.IsOpen)
            {
                kinectSensor.Close();
            }

            kinectSensor = null;
        }
    }

    // Handler for when the color frame reader's frame arrives.
    private void ColorFrameReader_FrameArrived(object sender, Kinect.ColorFrameArrivedEventArgs e)
    {
        if (Enabled())
        {
            UpdateTexture(e.FrameReference.AcquireFrame());
        }
    }

    // Update the texture to the latest frame from the reader
    private void UpdateTexture(Kinect.ColorFrame frame)
    {
        if (frame != null)
        {
            frame.CopyConvertedFrameDataToArray(textureData, Kinect.ColorImageFormat.Rgba);
            texture.LoadRawTextureData(textureData);
            texture.Apply();
            renderer.sharedMaterial.mainTexture = texture;
            frame.Dispose();
            frame = null;
        }
    }

}
