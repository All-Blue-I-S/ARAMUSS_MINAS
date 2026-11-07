
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class GazeboSinc : MonoBehaviour
{
    ROSConnection ros;

    [Header("ROS")]
    public string topicName = "/rov/pose";

    [Header("Referencial")]
    public Vector3 originUnity = Vector3.zero;

    Vector3 posicaoRos;
    Quaternion orientacaoRos;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        orientacaoRos = Quaternion.identity;

        ros.Subscribe<PoseStampedMsg>(
            topicName,
            PoseCallback
        );
    }

    void PoseCallback(PoseStampedMsg msg)
    {
        // Posição ROS
        double x = msg.pose.position.x;
        double y = msg.pose.position.y;
        double z = msg.pose.position.z;

        // ROS -> Unity
        posicaoRos = new Vector3(
            (float)-y,
            (float)z,
            (float)x
        );

        // Quaternion ROS
        double qx = msg.pose.orientation.x;
        double qy = msg.pose.orientation.y;
        double qz = msg.pose.orientation.z;
        double qw = msg.pose.orientation.w;

        // Conversão de orientação ROS -> Unity
        orientacaoRos = new Quaternion(
            (float)-qy,
            (float)qz,
            (float)-qx,
            (float)qw
        );
    }

    void Update()
    {
        transform.position = originUnity + posicaoRos;
        transform.rotation = orientacaoRos;
    }
}
