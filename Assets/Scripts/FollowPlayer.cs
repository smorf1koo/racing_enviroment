using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Vector3 offsetBack = new Vector3(0, 0.5f, 1); // Смещение камеры от игрока
    public GameObject player;  // Ссылка на игрока

    void LateUpdate()
    {
        if (player != null && player.activeInHierarchy)
        {
            transform.position = player.transform.position + player.transform.TransformDirection(offsetBack);
            transform.LookAt(player.transform.position);
        }
        else
        {
            Debug.LogError("Player is inactive or null!");
        }
    }
}