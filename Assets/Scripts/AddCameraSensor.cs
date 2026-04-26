using UnityEngine;

public class AddCameraSensor : MonoBehaviour
{
    void Start()
    {
        var camera = GetComponent<Camera>();
        if (camera == null)
        {
            Debug.LogError("Camera component not found on this object!");
            return;
        }

        Debug.Log("Camera found.");
    }
}
