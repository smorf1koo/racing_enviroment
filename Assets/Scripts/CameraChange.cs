using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChange : MonoBehaviour
{
    private Camera mainCamera;
    public Camera additionalCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            mainCamera.enabled = !mainCamera.enabled;
            additionalCamera.enabled = !additionalCamera.enabled;
        }
    }
}
