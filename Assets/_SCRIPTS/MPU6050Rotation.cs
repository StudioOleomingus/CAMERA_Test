using UnityEngine;

public class MPU6050Rotation : MonoBehaviour
{
    public SerialController serialController;

    [Range(0f, 0.99f)]
    public float smoothing = 0.5f;

    private Quaternion targetRotation = Quaternion.identity;
    private bool dataReceived = false;

    void Start()
    {
        if (serialController == null)
        {
            serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        }
    }

    void Update()
    {
        // Drain ALL pending messages, only use the latest one
        string latestMsg = null;
        string msg;
        while ((msg = serialController.ReadSerialMessage()) != null)
        {
            if (!msg.StartsWith("__"))
            {
                latestMsg = msg;
            }
        }

        if (latestMsg != null)
        {
            ProcessMessage(latestMsg);
        }

        if (dataReceived)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - smoothing);
        }
    }

    void ProcessMessage(string msg)
    {
        string[] parts = msg.Split(',');

        if (parts.Length == 3)
        {
            float yaw, pitch, roll;

            if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out yaw) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out pitch) &&
                float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out roll))
            {
                targetRotation = Quaternion.Euler(-pitch, -yaw, roll);
                dataReceived = true;
            }
        }
    }

    void OnMessageArrived(string msg)
    {
        // No longer needed but keep for compatibility
    }

    void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Serial connected." : "Serial connection failed.");
    }
}