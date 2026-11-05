using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ImagesPublisher : MonoBehaviour
{
    private Camera cameraToCapture;
    private RenderTexture renderTexture;
    ROSConnection ros;

    public string topicName = "/oak/rgb/image_raw";
    public float publishMessagePeriod = 1 / 30.0f;
    private float timeElapsed;

    // Resolução travada para estabilidade TCP e otimização do YOLO
    public int imageWidth = 640;
    public int imageHeight = 480;

    void Start()
    {
        cameraToCapture = GetComponent<Camera>();
        
        // Desacopla da resolução da tela para evitar crashes de index
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        cameraToCapture.targetTexture = renderTexture;
        
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessagePeriod)
        {
            Texture2D texture = CaptureImage();
            
            // GetPixels32 é brutalmente mais rápido pois já retorna bytes em vez de floats
            Color32[] pixels = texture.GetPixels32();
            byte[] msgArray = new byte[pixels.Length * 3];

            // Loop ajustado para usar as variáveis dinâmicas em vez de valores "hardcoded"
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    int srcIndex = (y * imageWidth) + x;
                    int dstIndex = ((imageHeight - 1 - y) * imageWidth + x) * 3; // Invertendo as linhas para o ROS

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

            image_msg.header.stamp.sec = (uint)Mathf.Floor(Time.time);
            image_msg.header.stamp.nanosec = (uint)Mathf.Floor((Time.time - Mathf.Floor(Time.time)) * 1e9f);
            image_msg.header.frame_id = "stereo_camera";

            ros.Publish(topicName, image_msg);

            timeElapsed = 0;
            Destroy(texture); // Crucial para não estourar a RAM
        }
    }

    private Texture2D CaptureImage()
    {
        cameraToCapture.Render();
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null; // Libera o contexto gráfico
        
        return texture;
    }
}
