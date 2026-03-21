using UnityEngine;

public class CarSplineStats : MonoBehaviour
{
    [SerializeField] private SplineCalculator splineCalculator;
    [SerializeField] private Transform carTransform;
    void Start()
    {
        splineCalculator = FindObjectOfType<SplineCalculator>();
        carTransform = GetComponent<Transform>();
    }

    /// <summary>
    /// Минимальное расстояние от машины до сплайна
    /// </summary>
    public float GetDistanceToSpline()
    {
        return splineCalculator.GetDistanceToSpline(carTransform.position);
    }

    /// <summary>
    /// Индекс ближайшей точки сплайна
    /// </summary>
    public int GetClosestSplineIndex()
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < splineCalculator.splinePoints.Length; i++)
        {
            float distance = Vector3.Distance(carTransform.position, splineCalculator.splinePoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    /// <summary>
    /// Процент прогресса вдоль сплайна
    /// </summary>
    public float GetProgressAlongSpline()
    {
        int index = GetClosestSplineIndex();
        return (float)index / (splineCalculator.splinePoints.Length - 1);
    }

    /// <summary>
    /// Угол между направлением машины и направлением сплайна
    /// </summary>
    public float GetAngleToSplineDirection()
    {
        int index = GetClosestSplineIndex();
        if (index < splineCalculator.splinePoints.Length - 1)
        {
            Vector3 splineDir = (splineCalculator.splinePoints[index + 1] - splineCalculator.splinePoints[index]).normalized;
            float angle = Vector3.SignedAngle(carTransform.forward, splineDir, Vector3.up);
            return angle;
        }
        return 0f;
    }

    /// <summary>
    /// Кривизна в ближайшей точке — чем больше угол поворота, тем выше значение
    /// </summary>
    public float GetLocalCurvature()
    {
        int i = GetClosestSplineIndex();
        if (i > 0 && i < splineCalculator.splinePoints.Length - 1)
        {
            Vector3 a = splineCalculator.splinePoints[i - 1];
            Vector3 b = splineCalculator.splinePoints[i];
            Vector3 c = splineCalculator.splinePoints[i + 1];

            Vector3 ab = (b - a).normalized;
            Vector3 bc = (c - b).normalized;
            return Vector3.Angle(ab, bc); 
        }
        return 0f;
    }
}
