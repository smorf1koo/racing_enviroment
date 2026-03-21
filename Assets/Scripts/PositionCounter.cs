using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PositionCounter : MonoBehaviour
{
    [SerializeField] CarControllerAgent[] carAgents;
    [SerializeField] float[] carsProgress;
    [SerializeField] private int[] carsCheckpoints;
    [SerializeField] int playerPosition = 1; // По умолчанию 1
    [SerializeField] float playerProgress;
    [SerializeField] private int playerCheckpoints;
    [SerializeField] private Text playerPose;
    [SerializeField] private Text allPlayers;
    
    private CarControllerAgent playerAgent;

    void Start()
    {
        StartCoroutine(InitializeWithDelay()); // Используем корутину для задержки инициализации
    }

    private IEnumerator InitializeWithDelay()
    {
        // Ждем конец кадра, чтобы все объекты успели инициализироваться
        yield return new WaitForEndOfFrame();
        GetCarAgents();
    }

    private void GetCarAgents()
    {
        carAgents = FindObjectsOfType<CarControllerAgent>()?
            .Where(agent => agent != null && agent.enabled)
            .ToArray();

        if (carAgents == null || carAgents.Length == 0)
        {
            Debug.LogWarning("No car agents found!");
            return;
        }

        carsProgress = new float[carAgents.Length];
        carsCheckpoints = new int[carAgents.Length];
        
        foreach (var agent in carAgents)
        {
            if (agent == null) continue;

            try 
            {
                if (agent.IsPlayer())
                {
                    playerAgent = agent;
                    playerProgress = agent.ProgressAgent();
                    playerCheckpoints = agent.ChecksOver();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing agent: {e.Message}");
                continue;
            }
        }
    }

    void Update()
    {
        if (carAgents == null || carAgents.Length == 0) return;
        UpdatePositions();
        playerPose.text = playerPosition.ToString();
        allPlayers.text = carAgents.Length.ToString();
    }

    private void UpdatePositions()
    {
        int validAgentsCount = 0;

        // Обновляем прогресс всех машин с проверкой на null
        for (int i = 0; i < carAgents.Length; i++)
        {
            if (carAgents[i] == null || !carAgents[i].enabled) continue;

            try 
            {
                carsProgress[i] = carAgents[i].ProgressAgent();
                carsCheckpoints[i] = carAgents[i].ChecksOver();
                validAgentsCount++;

                if (carAgents[i].IsPlayer())
                {
                    playerProgress = carsProgress[i];
                    playerCheckpoints = carsCheckpoints[i];
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating agent progress: {e.Message}");
                continue;
            }
        }

        if (validAgentsCount == 0) return;

        // Создаем список только валидных агентов
        var agentsData = new List<CarAgentData>();
        for (int i = 0; i < carAgents.Length; i++)
        {
            if (carAgents[i] != null && carAgents[i].enabled)
            {
                agentsData.Add(new CarAgentData
                {
                    Agent = carAgents[i],
                    Progress = carsProgress[i],
                    Checkpoints = carsCheckpoints[i]
                });
            }
        }

        // Сортируем сначала по количеству чекпоинтов, затем по прогрессу
        var sortedAgents = agentsData
            .OrderByDescending(x => x.Checkpoints)
            .ThenByDescending(x => x.Progress)
            .ToList();

        // Находим позицию игрока
        for (int i = 0; i < sortedAgents.Count; i++)
        {
            if (sortedAgents[i].Agent.IsPlayer())
            {
                playerPosition = i + 1;
                break;
            }
        }
    }

    public int GetPlayerPosition()
    {
        return playerPosition;
    }

    public float GetPlayerProgress()
    {
        return playerProgress;
    }
    private class CarAgentData
    {
        public CarControllerAgent Agent { get; set; }
        public float Progress { get; set; }
        public int Checkpoints { get; set; }
    }
}