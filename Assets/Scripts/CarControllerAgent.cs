using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerAgent : MonoBehaviour
{
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private int maxSteps = 3000;
    private float forwardSpeedReward = 0.01f;
    private float perStepPenalty = -0.001f;
    [SerializeField] private float timer = 30f;
    [SerializeField] private Text value;
    [SerializeField] private Text checksGone;
    [SerializeField] private Text allChecks;
    [SerializeField] private Text rewardNum;
    [SerializeField] private Text speedText;
    [SerializeField] private CarSplineStats carSplineStats;
    private bool start = false;
    private int checksOver = 0;

    private int stepCount = 0;
    private CarController carController;
    private SplineCalculator splineCalculator;
    private Rigidbody rb;
    private AIController aiController;
    [SerializeField] private bool isPlayer = false;
    private float spentTime = 0f;
    private float totalErrors = 0f;
    private float totalSpeed = 0f;
    private int speedMeasurementsCount = 0;

    private float _cumulativeReward = 0f;

    private void Awake() {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();
        carSplineStats = GetComponent<CarSplineStats>();
        splineCalculator = FindObjectOfType<SplineCalculator>();
        aiController = GetComponent<AIController>();
        if (rb != null)
        {
            Debug.Log("rb is not null");
        }
    }
    private void Start()
    {
        checksOver = 0;
        start = true;
        trackCheckpoints.OnPlayerCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
        trackCheckpoints.OnPlayerWrongCheckpoint += TrackCheckpoints_OnCarWrongCheckpoint;
    }

    private void TrackCheckpoints_OnCarCorrectCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform){
            AddReward(10f + ((float)checksOver / 5f));
            timer += 1f;
            checksOver++;
        }
    }

    public int ChecksOver()
    {
        return checksOver;
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }
    private void TrackCheckpoints_OnCarWrongCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform){
            AddReward(-2f);
            totalErrors -= 2f;
        }
    }

    private void FixedUpdate()
    {
        if (!start) return;

        float forwardAmount, turnAmount;
        if (isPlayer)
        {
            forwardAmount = Input.GetAxis("Vertical");
            turnAmount = Input.GetAxis("Horizontal");
        }
        else if (aiController != null)
        {
            forwardAmount = aiController.InputVerticalAI();
            turnAmount = aiController.InputHorizontalAI();
        }
        else
        {
            return;
        }

        ApplyAction(forwardAmount, turnAmount);
    }

    void Update()
    {
        if (start == true)
        {
            if (splineCalculator.GetDistanceToSpline(transform.position) < 2) AddReward(0.005f);
            if ((carController.CurrentSpeed() * 3.6 >= 20f) && (carController.IsMovingForward())) AddReward(0.005f);
            totalSpeed += carController.CurrentSpeed() * 3.6f;
            speedMeasurementsCount++;
            speedText.text = (carController.CurrentSpeed() * 3.6).ToString("0.0");
            timer -= Time.deltaTime;
            spentTime += Time.deltaTime;
            value.text = timer.ToString("0.00");
            checksGone.text = checksOver.ToString();
            allChecks.text = TrackCheckpoints.GetChecks().ToString();
            rewardNum.text = GetCumulativeReward().ToString("0.000");
            if (timer <= 0)
            {
                Debug.Log(spentTime);
                AddReward(-20f);
                totalErrors -= 20f;
                Debug.Log(totalErrors);
                Debug.Log(GetCumulativeReward());
                Debug.Log(totalSpeed/speedMeasurementsCount);
                EndEpisode();
            } else if (checksOver == TrackCheckpoints.GetChecks())
            {
                Debug.Log(spentTime);
                AddReward(5000f);
                Debug.Log(totalErrors);
                Debug.Log(GetCumulativeReward());
                Debug.Log(totalSpeed/speedMeasurementsCount);
                EndEpisode();
            }
            if (carController.CurrentSpeed() < 0.5f)
                AddReward(-0.001f);
                totalErrors -= 0.001f;
        }
    }

    public void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f));
        transform.forward = spawnPosition.forward;
        trackCheckpoints.ResetCheckpoint(transform);
        checksOver = 0;
        timer = 30f;
        spentTime = 0f;
        totalErrors = 0f;

        stepCount = 0;
        _cumulativeReward = 0f;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    public float ProgressAgent()
    {
        return carSplineStats.GetProgressAlongSpline();
    }

    public void AddReward(float reward)
    {
        _cumulativeReward += reward;
    }

    public float GetCumulativeReward()
    {
        return _cumulativeReward;
    }

    public void EndEpisode()
    {
        _cumulativeReward = 0f;
        OnEpisodeBegin();
    }

    /// <summary>
    /// Возвращает вектор наблюдений для gRPC/TCP сервера (7 float).
    /// Порядок: distance_to_spline, progress, angle_to_spline, curvature,
    /// distance_to_checkpoint, direction_dot, speed.
    /// </summary>
    public float[] GetObservationVector()
    {
        var obs = new float[7];
        obs[0] = carSplineStats.GetDistanceToSpline();
        obs[1] = carSplineStats.GetProgressAlongSpline();
        obs[2] = carSplineStats.GetAngleToSplineDirection();
        obs[3] = carSplineStats.GetLocalCurvature();

        var nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);
        obs[4] = nextCheckpoint != null ? trackCheckpoints.GetDistanceToNextCheckpoint(transform) : 0f;
        obs[5] = nextCheckpoint != null ? Vector3.Dot(transform.forward, nextCheckpoint.transform.forward) : 0f;

        obs[6] = rb.velocity.magnitude;
        return obs;
    }

    /// <summary>
    /// Применяет действие из внешнего клиента (gRPC/TCP).
    /// forwardAmount, turnAmount в [-1, 1].
    /// </summary>
    public void ApplyAction(float forwardAmount, float turnAmount)
    {
        if (forwardAmount > 0f)
            AddReward(forwardSpeedReward);

        carController.SetInput(forwardAmount, turnAmount);
        AddReward(perStepPenalty);
        totalErrors -= perStepPenalty;

        stepCount++;
        if (stepCount >= maxSteps)
        {
            AddReward(-100f);
            totalErrors -= 100f;
            EndEpisode();
        }
    }

    /// <summary>
    /// Эпизод завершён (таймер, все чекпоинты или maxSteps).
    /// </summary>
    public bool IsEpisodeDone()
    {
        if (timer <= 0f) return true;
        if (checksOver == TrackCheckpoints.GetChecks()) return true;
        if (stepCount >= maxSteps) return true;
        return false;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-10f);
            totalErrors -= 10f;
        }
        if (other.gameObject.TryGetComponent<Player>(out Player player))
        {
            AddReward(-10f);
            totalErrors -= 10f;
        }
    }
    private void OnCollisionStay(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.1f);
            totalErrors -= 0.1f;
        }
    }
}
