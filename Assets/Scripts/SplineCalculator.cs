using UnityEngine;
using System.Collections.Generic;

public class SplineCalculator : MonoBehaviour
{
    public List<Transform> controlPoints = new List<Transform>();
    public int resolution = 20; // Количество точек между контрольными точками
    public float lineWidth = 0.5f;
    public Material splineMaterial;
    public bool drawGizmos = true;
    public bool smoothTangents = true;
    
    private LineRenderer lineRenderer;
    public Vector3[] splinePoints;

    void Start()
    {
        Transform splinePointsTransform = transform.Find("SplinePoints");

        foreach (Transform pointSingleTransform in splinePointsTransform)
        {
            controlPoints.Add(pointSingleTransform);
        }
        InitializeLineRenderer();
        GenerateContinuousBezierSpline();
        VisualizeSpline();
    }

    void Update()
    {
        if (AnyControlPointChanged())
        {
            GenerateContinuousBezierSpline();
            VisualizeSpline();
        }
    }

    bool AnyControlPointChanged()
    {
        foreach (var point in controlPoints)
        {
            if (point != null && point.hasChanged)
            {
                point.hasChanged = false;
                return true;
            }
        }
        return false;
    }

    public float GetDistanceToSpline(Vector3 point)
    {
        if (splinePoints == null || splinePoints.Length < 2)
            return float.MaxValue;

        float minDistance = float.MaxValue;

        for (int i = 0; i < splinePoints.Length - 1; i++)
        {
            float segmentDistance = DistanceToSegment(point, splinePoints[i], splinePoints[i + 1]);
            if (segmentDistance < minDistance)
            {
                minDistance = segmentDistance;
            }
        }

        return minDistance;
    }

    private float DistanceToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        Vector3 segment = segmentEnd - segmentStart;
        Vector3 toPoint = point - segmentStart;

        float dot = Vector3.Dot(toPoint, segment.normalized);
        dot = Mathf.Clamp(dot, 0, segment.magnitude);

        Vector3 closestPoint = segmentStart + segment.normalized * dot;
        return Vector3.Distance(point, closestPoint);
    }

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.material = splineMaterial != null ? splineMaterial : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
    }

    void GenerateContinuousBezierSpline()
    {
        if (controlPoints.Count < 2)
        {
            splinePoints = new Vector3[0];
            return;
        }

        List<Vector3> points = new List<Vector3>();

        points.Add(controlPoints[0].position);

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 startPoint = controlPoints[i].position;
            Vector3 endPoint = controlPoints[i + 1].position;

            Vector3 startTangent = CalculateStartTangent(i);
            Vector3 endTangent = CalculateEndTangent(i);

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                points.Add(CalculateHermitePoint(t, startPoint, endPoint, startTangent, endTangent));
            }
        }

        splinePoints = points.ToArray();
    }

    Vector3 CalculateStartTangent(int index)
    {
        if (!smoothTangents || controlPoints.Count < 2)
            return Vector3.zero;
            if (index == 0)
        {
            return (controlPoints[index + 1].position - controlPoints[index].position) * 0.5f;
        }
        else if (index == controlPoints.Count - 1)
        {
            return (controlPoints[index].position - controlPoints[index - 1].position) * 0.5f;
        }
        else
        {
            return (controlPoints[index + 1].position - controlPoints[index - 1].position) * 0.5f;
        }
    }

    Vector3 CalculateEndTangent(int index)
    {
        if (!smoothTangents || controlPoints.Count < 2)
            return Vector3.zero;

        if (index >= controlPoints.Count - 2)
        {
            return (controlPoints[index + 1].position - controlPoints[index].position) * 0.5f;
        }
        else
        {
            return (controlPoints[index + 2].position - controlPoints[index].position) * 0.5f;
        }
    }

    Vector3 CalculateHermitePoint(float t, Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1)
    {
        t = Mathf.Clamp01(t);
        float t2 = t * t;
        float t3 = t2 * t;
        
        return (2f*t3 - 3f*t2 + 1f) * p0 +
               (t3 - 2f*t2 + t) * m0 +
               (-2f*t3 + 3f*t2) * p1 +
               (t3 - t2) * m1;
    }

    public void VisualizeSpline()
    {
        if (lineRenderer == null || splinePoints == null || splinePoints.Length == 0)
            return;

        lineRenderer.positionCount = splinePoints.Length;
        lineRenderer.SetPositions(splinePoints);
        
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.red;
        
        lineRenderer.material.mainTextureScale = new Vector2(1f / lineWidth, 1);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || controlPoints.Count == 0)
            return;

        Gizmos.color = Color.magenta;
        foreach (var point in controlPoints)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.3f);
        }

        Gizmos.color = Color.gray;
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            if (controlPoints[i] != null && controlPoints[i+1] != null)
                Gizmos.DrawLine(controlPoints[i].position, controlPoints[i+1].position);
        }

        if (splinePoints != null && splinePoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < splinePoints.Length - 1; i++)
            {
                Gizmos.DrawLine(splinePoints[i], splinePoints[i+1]);
            }
        }
    }
}