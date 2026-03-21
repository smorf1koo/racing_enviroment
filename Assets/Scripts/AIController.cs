using UnityEngine;

public class AIController : MonoBehaviour
{
    private CarController carController;
    private Rigidbody rb;
    
    [Header("Spline Following Settings")]
    [SerializeField] private SplineCalculator splineCalculator;
    public float lookAheadDistance = 5f;
    public float maxSteeringAngle = 45f;
    public float speedAdjustmentCurve = 1.5f;
    public float positionPrediction = 0.5f;
    
    private int currentSplineIndex;
    private float[] splineDistances;
    private Vector3 targetPosition;
    private float currentHorizontalInput;
    private float currentVerticalInput;

    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();
        
        if(splineCalculator == null)
        {
            splineCalculator = FindObjectOfType<SplineCalculator>();
            if(splineCalculator == null)
            {
                Debug.LogError("SplineCalculator не найден в сцене!");
                return;
            }
        }
        
        InitializeSplineData();
    }

    void InitializeSplineData()
    {
        if(splineCalculator == null || splineCalculator.splinePoints == null) return;
        
        splineDistances = new float[splineCalculator.splinePoints.Length];
        float totalDistance = 0f;
        
        for(int i = 1; i < splineCalculator.splinePoints.Length; i++)
        {
            totalDistance += Vector3.Distance(
                splineCalculator.splinePoints[i-1], 
                splineCalculator.splinePoints[i]
            );
            splineDistances[i] = totalDistance;
        }
    }

    void Update()
    {
        if(splineCalculator == null || splineCalculator.splinePoints == null) return;
        
        UpdateNavigation();
        CalculateSteering();
        AdjustSpeed();
        ApplyInputs();
    }

    void UpdateNavigation()
    {
        float minDistance = float.MaxValue;
        for(int i = 0; i < splineCalculator.splinePoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, splineCalculator.splinePoints[i]);
            if(distance < minDistance)
            {
                minDistance = distance;
                currentSplineIndex = i;
            }
        }

        int targetIndex = (currentSplineIndex + Mathf.FloorToInt(lookAheadDistance)) % splineCalculator.splinePoints.Length;
        targetPosition = splineCalculator.splinePoints[targetIndex];
    }

    void CalculateSteering()
    {
        Vector3 predictedPosition = transform.position + rb.velocity * positionPrediction;
        Vector3 directionToTarget = (targetPosition - predictedPosition).normalized;
        
        float angleToTarget = Vector3.SignedAngle(
            transform.forward, 
            directionToTarget, 
            Vector3.up
        );

        currentHorizontalInput = Mathf.Clamp(angleToTarget / maxSteeringAngle, -1f, 1f);
    }

    void AdjustSpeed()
    {
        float curvature = Mathf.Abs(currentHorizontalInput);
        float speedFactor = Mathf.Pow(1 - curvature, speedAdjustmentCurve);
        currentVerticalInput = Mathf.Clamp01(speedFactor);
    }

    void ApplyInputs()
    {
        // Сглаживание ввода
        float smoothHorizontal = Mathf.Lerp(
            carController.horizontalInput,
            currentHorizontalInput,
            Time.deltaTime * 5f
        );
        
        float smoothVertical = Mathf.Lerp(
            carController.verticalInput,
            currentVerticalInput,
            Time.deltaTime * 3f
        );

        //carController.SetAIInput(smoothHorizontal, smoothVertical);
    }
    public float InputHorizontalAI()
    {
        return Mathf.Lerp(
            carController.horizontalInput,
            currentHorizontalInput,
            Time.deltaTime * 5f);
    }
    public float InputVerticalAI()
    {
        return Mathf.Lerp(
            carController.verticalInput,
            currentVerticalInput,
            Time.deltaTime * 3f);
    }
    void OnDrawGizmosSelected()
    {
        if(splineCalculator != null && splineCalculator.splinePoints != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }
    }
}

