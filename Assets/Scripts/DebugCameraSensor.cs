using UnityEngine;
using Unity.MLAgents.Sensors;

public class DebugCameraSensor : MonoBehaviour
{
    void Start()
    {
        var sensorComponent = GetComponent<CameraSensorComponent>();
        if (sensorComponent != null)
        {
            Debug.Log("CameraSensorComponent найден.");
            if (sensorComponent.Camera == null)
            {
                Debug.LogError("Камера не назначена в CameraSensorComponent!");
            }
            else
            {
                Debug.Log($"Камера назначена: {sensorComponent.Camera.name}");
            }
        }
        else
        {
            Debug.LogError("CameraSensorComponent не найден на объекте!");
        }
    }
}

