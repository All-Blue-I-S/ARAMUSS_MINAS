using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System;

public class ImagePublisher : MonoBehaviour
{
    public Camera cameraToCapture;
    public Camera cameraInferior;

    private RenderTexture renderTextureFrontal;
    private RenderTexture renderTextureInferior;

    ROSConnection ros;

    public string topicNameFrontal = "/oak/rgb/image_raw";
    public string topicNameInferior = "/oak/rgb/image_inferior_raw";

    public float publishMessagePeriod = 1 / 30.0f;
    private float timeElapsed;

    // Resolução travada para estabilidade TCP e otimização do YOLO
    public int imageWidth = 640;
    public int imageHeight = 480;

    void Start()
    {
        // Desacopla da resolução da tela para evitar crashes de index
        renderTextureFrontal =
            new RenderTexture(imageWidth, imageHeight, 24);

        cameraToCapture.targetTexture = renderTextureFrontal;

        renderTextureInferior =
            new RenderTexture(imageWidth, imageHeight, 24);

        cameraInferior.targetTexture = renderTextureInferior;

        ros = ROSConnection.GetOrCreateInstance();

        ros.RegisterPublisher<ImageMsg>(topicNameFrontal);
        ros.RegisterPublisher<ImageMsg>(topicNameInferior);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessagePeriod)
        {
            pubImageFrontal();
            pubImageInferior();

            timeElapsed = 0;
        }
    }

    private Texture2D CaptureImageInferior()
    {
        cameraInferior.Render();

        Texture2D texture = new Texture2D(
            renderTextureInferior.width,
            renderTextureInferior.height,
            TextureFormat.RGB24,
            false
        );

        RenderTexture.active = renderTextureInferior;

        texture.ReadPixels(
            new Rect(
                0,
                0,
                renderTextureInferior.width,
                renderTextureInferior.height
            ),
            0,
            0
        );

        texture.Apply();

        RenderTexture.active = null;

        return texture;
    }

    private Texture2D CaptureImageFrontal()
    {
        cameraToCapture.Render();

        Texture2D texture = new Texture2D(
            renderTextureFrontal.width,
            renderTextureFrontal.height,
            TextureFormat.RGB24,
            false
        );

        RenderTexture.active = renderTextureFrontal;

        texture.ReadPixels(
            new Rect(
                0,
                0,
                renderTextureFrontal.width,
                renderTextureFrontal.height
            ),
            0,
            0
        );

        texture.Apply();

        RenderTexture.active = null;

        return texture;
    }

    private void pubImageInferior()
    {
        Texture2D texture = CaptureImageInferior();

        Color32[] pixels = texture.GetPixels32();
        byte[] msgArray = new byte[pixels.Length * 3];

        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                int srcIndex = (y * imageWidth) + x;

                int dstIndex =
                    ((imageHeight - 1 - y) * imageWidth + x) * 3;

                Color32 p = pixels[srcIndex];

                msgArray[dstIndex] = p.r;
                msgArray[dstIndex + 1] = p.g;
                msgArray[dstIndex + 2] = p.b;
            }
        }

        ImageMsg image_msg = new ImageMsg
        {
            data = msgArray,
            height = (uint)imageHeight,
            width = (uint)imageWidth,
            encoding = "rgb8",
            step = (uint)(imageWidth * 3)
        };

        double currentTime = Time.timeAsDouble;

        image_msg.header.stamp.sec =
            (int)Math.Floor(currentTime);

        image_msg.header.stamp.nanosec =
            (uint)((currentTime - Math.Floor(currentTime)) * 1e9);

        image_msg.header.frame_id = "camera_inferior";

        ros.Publish(topicNameInferior, image_msg);

        Destroy(texture);
    }

    private void pubImageFrontal()
    {
        Texture2D texture = CaptureImageFrontal();

        Color32[] pixels = texture.GetPixels32();
        byte[] msgArray = new byte[pixels.Length * 3];

        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                int srcIndex = (y * imageWidth) + x;

                int dstIndex =
                    ((imageHeight - 1 - y) * imageWidth + x) * 3;

                Color32 p = pixels[srcIndex];

                msgArray[dstIndex] = p.r;
                msgArray[dstIndex + 1] = p.g;
                msgArray[dstIndex + 2] = p.b;
            }
        }

        ImageMsg image_msg = new ImageMsg
        {
            data = msgArray,
            height = (uint)imageHeight,
            width = (uint)imageWidth,
            encoding = "rgb8",
            step = (uint)(imageWidth * 3)
        };

        double currentTime = Time.timeAsDouble;

        image_msg.header.stamp.sec =
            (int)Math.Floor(currentTime);

        image_msg.header.stamp.nanosec =
            (uint)((currentTime - Math.Floor(currentTime)) * 1e9);

        image_msg.header.frame_id = "stereo_camera";

        ros.Publish(topicNameFrontal, image_msg);

        Destroy(texture);
    }
}
