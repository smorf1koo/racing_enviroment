using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class VehicleMoving : MonoBehaviour
{
    public float vehicleSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * vehicleSpeed * Time.deltaTime);
    }
}
