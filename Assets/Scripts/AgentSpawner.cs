using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsToSpawn; // Префаб, который нужно создать
    [SerializeField] private Material[] materials; // Префаб, который нужно создать
    [SerializeField] private Transform parentObject;   // Родительский объект, к которому будет привязан префаб
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; // Позиция относительно родителя
    [SerializeField] private Vector3 spawnRotation = Vector3.zero; // Угол поворота относительно родителя
    [SerializeField] private Vector3 spawnScale = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        //SpawnPrefab(PrefabSpawner.GetNumCar());
        ChangeMaterial(PrefabSpawner.GetNumMaterial());
    }

    public void SpawnPrefab(int numPrefab)
    {
        // if (prefabsToSpawn == null)
        // {
        //     Debug.LogError("Prefab to spawn is not assigned!");
        //     return;
        // }

        // if (parentObject == null)
        // {
        //     Debug.LogError("Parent object is not assigned!");
        //     return;
        // }

        // // Удаляем все дочерние объекты родительского объекта
        // foreach (Transform child in parentObject)
        // {
        //     Destroy(child.gameObject);
        // }

        // Создаём экземпляр префаба
        GameObject spawnedObject = Instantiate(prefabsToSpawn[numPrefab]);

        // Устанавливаем родительский объект
        spawnedObject.transform.SetParent(parentObject, false); // false сохраняет локальные координаты

        // Устанавливаем позицию и поворот
        spawnedObject.transform.localPosition = spawnPosition;
        spawnedObject.transform.localRotation = Quaternion.Euler(spawnRotation);
        spawnedObject.transform.localScale = spawnScale;
    }

    public void ChangeMaterial(int numMaterial)
    {
        // if (parentObject == null)
        // {
        //     Debug.LogError("Parent object is not assigned!");
        //     return;
        // }

        // if (materials == null)
        // {
        //     Debug.LogWarning("No material provided to apply!");
        //     return;
        // }

        // // Проверяем, есть ли дочерние объекты
        // if (parentObject.childCount == 0)
        // {
        //     Debug.LogError("No child objects under the parent to change material for!");
        //     return;
        // }

        // Меняем материал у всех рендеров дочернего объекта
        foreach (Transform child in parentObject)
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = materials[numMaterial];
            }
        }
    }
}
