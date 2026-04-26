using UnityEngine;

public class DebugCameraSensor : MonoBehaviour
{
    void Start()
    {
        var camera = GetComponent<Camera>();
        if (camera != null)
        {
            Debug.Log($"Camera found: {camera.name}");
        }
        else
        {
            Debug.LogError("Camera not found on this object!");
        }
    }
}
