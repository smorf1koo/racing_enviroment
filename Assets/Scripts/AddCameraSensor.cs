using UnityEngine;
using Unity.MLAgents.Sensors;

public class AddCameraSensor : MonoBehaviour
{
    void Start()
    {
        // Проверяем, есть ли компонент Camera на объекте
        var camera = GetComponent<Camera>();
        if (camera == null)
        {
            Debug.LogError("Camera component not found on this object!");
            return;
        }

        // Добавляем CameraSensorComponent
        var sensor = gameObject.AddComponent<CameraSensorComponent>();
        sensor.Camera = camera; // Назначаем камеру
        sensor.SensorName = "AgentCameraSensor"; // Указываем имя сенсора
        sensor.Width = 84; // Устанавливаем ширину наблюдения
        sensor.Height = 84; // Устанавливаем высоту наблюдения
        sensor.ObservationStacks = 1; // Указываем количество стэков наблюдения
        sensor.CompressionType = SensorCompressionType.PNG; // Тип сжатия
        //sensor.ObservationType = CameraSensorComponent.ObservationType.Default; // Тип наблюдения

        Debug.Log("CameraSensorComponent успешно добавлен и настроен!");
    }
}

