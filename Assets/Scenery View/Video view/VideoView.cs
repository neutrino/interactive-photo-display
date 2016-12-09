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
    }

    void Start()
    {
        // Set the opacity of the material
        Color color = renderer.sharedMaterial.color;
        color.a = opacity;
        renderer.sharedMaterial.color = color;

        StartKinect();

        if (kinectSensor != null)
        {
            // Initialize the texture
            var frameDesc = kinectSensor.ColorFrameSource.CreateFrameDescription(Kinect.ColorImageFormat.Rgba);
            texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            textureData = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];

            // Rescale the transform to match the aspect ratio of the image
            Vector3 scale = transform.localScale;
            scale.x = -scale.y * ((float)texture.width / (float)texture.height);
            transform.localScale = scale;
        }
    }
    void OnDestroy()
    {
        CloseKinect();
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
        UpdateTexture(e.FrameReference.AcquireFrame());
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BodyTracker bodyTracker = FindObjectOfType<BodyTracker>();
        Kinect.Body body = bodyTracker.NearestBody();
        if (body != null)
        {
            Vector2 pos = bodyTracker.JointPositionOnScreen(body, Kinect.JointType.HandRight);
            Vector2 posOnVideo = transform.position;
            posOnVideo.x -= Mathf.Abs(transform.localScale.x) / 2f;
            posOnVideo.x += pos.x * Mathf.Abs(transform.localScale.x);
            posOnVideo.y += Mathf.Abs(transform.localScale.y) / 2f;
            posOnVideo.y -= pos.y * Mathf.Abs(transform.localScale.y);
            Gizmos.DrawSphere(posOnVideo, 0.5f);
        }
    }

}
