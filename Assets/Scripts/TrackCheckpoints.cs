using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour {

    public static int numChecks;
    // Объявляем события с информацией о машине, проходящей чекпоинт
    public event EventHandler<CarCheckpointEventArgs> OnPlayerCorrectCheckpoint;
    public event EventHandler<CarCheckpointEventArgs> OnPlayerWrongCheckpoint;

    // Аргументы события для передачи carTransform
    public class CarCheckpointEventArgs : EventArgs
    {
        public Transform carTransform;
    }
    public CarController[] carControllers;
    private List<Transform> carTransformList;

    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;

    private void Awake() {
        // Находим дочерний объект с чекпоинтами и добавляем их в список
        Transform checkpointsTransform = transform.Find("Checkpoints");

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointsTransform) {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();

            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);
        }
        numChecks = checkpointSingleList.Count;

        InitializeCarTransformList();

        // Инициализируем индексы для отслеживания следующего чекпоинта для каждой машины
        nextCheckpointSingleIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList) {
            nextCheckpointSingleIndexList.Add(0);
        }
    }
    public static int GetChecks()
    {
        return numChecks;
    }
    private void InitializeCarTransformList()
    {
        // Находим все объекты с компонентом CarController (или другим компонентом, обозначающим машину)
        carTransformList = new List<Transform>();

        foreach (CarController carController in carControllers)
        {
            carTransformList.Add(carController.transform);
        }

        if (carTransformList.Count == 0)
        {
            Debug.LogWarning("No cars found in the scene! Ensure cars are correctly set up.");
        }
    }
    // Метод, вызываемый при прохождении чекпоинта
    public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform) {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex == -1) {
            Debug.LogWarning("Car not found in carTransformList");
            return;
        }

        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[carIndex];
        if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex) {
            // Правильный чекпоинт
            //Debug.Log("Correct checkpoint");

            // Скрываем текущий чекпоинт
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            correctCheckpointSingle.Hide();

            // Обновляем индекс следующего чекпоинта для машины
            nextCheckpointSingleIndexList[carIndex] = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;

            // Вызываем событие для правильного чекпоинта
            OnPlayerCorrectCheckpoint?.Invoke(this, new CarCheckpointEventArgs { carTransform = carTransform });
        } else {
            // Неправильный чекпоинт
            //Debug.Log("Wrong checkpoint");

            // Показываем правильный чекпоинт
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            correctCheckpointSingle.Show();

            // Вызываем событие для неправильного чекпоинта
            OnPlayerWrongCheckpoint?.Invoke(this, new CarCheckpointEventArgs { carTransform = carTransform });
        }
    }
    // Метод для получения расстояния до следующего чекпоинта
    public float GetDistanceToNextCheckpoint(Transform carTransform) {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex == -1) {
            Debug.LogWarning("Car not found in carTransformList");
            return -1f; // Возвращаем -1 как сигнал ошибки
        }

        int nextCheckpointIndex = nextCheckpointSingleIndexList[carIndex];
        Transform nextCheckpointTransform = checkpointSingleList[nextCheckpointIndex].transform;

        return Vector3.Distance(carTransform.position, nextCheckpointTransform.position);
    }
    // Метод для получения следующего чекпоинта для машины
    public CheckpointSingle GetNextCheckpoint(Transform carTransform) {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex == -1) {
            Debug.LogWarning("Car not found in carTransformList");
            return null;
        }
        int nextCheckpointIndex = nextCheckpointSingleIndexList[carIndex];
        return checkpointSingleList[nextCheckpointIndex];
    }

    // Метод для сброса состояния чекпоинтов для машины
    public void ResetCheckpoint(Transform carTransform) {
        int carIndex = carTransformList.IndexOf(carTransform);
        if (carIndex == -1) {
            Debug.LogWarning("Car not found in carTransformList");
            return;
        }
        nextCheckpointSingleIndexList[carIndex] = 0;
    }
}
