using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerAgent : MonoBehaviour
{
    [SerializeField] private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private int maxSteps = 6000;
    private float forwardSpeedReward = 0.01f;
    private float perStepPenalty = -0.001f;
    [SerializeField] private float timer = 120f;
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
    private bool _externalControl = false;
    private bool _episodeDone = false;
    private bool _isTouchingWall = false;
    private float _wallContactStreakSec = 0f;
    private float _isMovingBackwardFlag = 0f;

    public void SetExternalControl(bool value) => _externalControl = value;

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
            timer += 5f;
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
        if (!start || _externalControl) return;

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
        // Во время обучения физика шагается вручную из gRPC,
        // поэтому Update() нельзя выполнять, иначе таймер/награды будут считаться дважды.
        if (_externalControl)
        {
            UpdateUi();
            return;
        }

        if (start == true)
        {
            if (splineCalculator.GetDistanceToSpline(transform.position) < 2) AddReward(0.005f);
            if ((carController.CurrentSpeed() * 3.6 >= 20f) && (carController.IsMovingForward())) AddReward(0.005f);
            totalSpeed += carController.CurrentSpeed() * 3.6f;
            speedMeasurementsCount++;
            timer -= Time.deltaTime;
            spentTime += Time.deltaTime;
            if (_isTouchingWall)
                _wallContactStreakSec += Time.deltaTime;
            UpdateMovingBackwardFlag();
            UpdateUi();
            if (timer <= 0)
            {
                Debug.Log(spentTime);
                AddReward(-20f);
                totalErrors -= 20f;
                Debug.Log(totalErrors);
                Debug.Log(GetCumulativeReward());
                Debug.Log(totalSpeed/speedMeasurementsCount);
                if (_externalControl)
                    _episodeDone = true;
                else
                    EndEpisode();
            } else if (checksOver == TrackCheckpoints.GetChecks())
            {
                Debug.Log(spentTime);
                AddReward(5000f);
                Debug.Log(totalErrors);
                Debug.Log(GetCumulativeReward());
                Debug.Log(totalSpeed/speedMeasurementsCount);
                if (_externalControl)
                    _episodeDone = true;
                else
                    EndEpisode();
            }
            if (carController.CurrentSpeed() < 0.5f)
            {
                AddReward(-0.001f);
                totalErrors -= 0.001f;
            }
        }
    }

    /// <summary>
    /// Выполняет “тик” наград/таймера/признаков ровно один раз после Physics.Simulate(dt).
    /// Вызывается из UnityRacingGrpcServer при external control.
    /// </summary>
    public void TickFromGrpc(float dt)
    {
        if (!start)
            return;

        // Шэйпинг по близости к сплайну/скорости.
        if (splineCalculator.GetDistanceToSpline(transform.position) < 2)
            AddReward(0.005f);

        if ((carController.CurrentSpeed() * 3.6 >= 20f) && (carController.IsMovingForward()))
            AddReward(0.005f);

        totalSpeed += carController.CurrentSpeed() * 3.6f;
        speedMeasurementsCount++;

        timer -= dt;
        spentTime += dt;

        if (_isTouchingWall)
            _wallContactStreakSec += dt;

        UpdateMovingBackwardFlag();

        // Условия завершения эпизода.
        if (timer <= 0)
        {
            AddReward(-20f);
            totalErrors -= 20f;
            _episodeDone = true; // в gRPC-режиме завершаем эпизод флагом
        }
        else if (checksOver == TrackCheckpoints.GetChecks())
        {
            AddReward(5000f);
            _episodeDone = true;
        }

        if (carController.CurrentSpeed() < 0.5f)
        {
            AddReward(-0.001f);
            totalErrors -= 0.001f;
        }

        UpdateUi();
    }

    public void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f));
        transform.forward = spawnPosition.forward;
        trackCheckpoints.ResetCheckpoint(transform);
        checksOver = 0;
        timer = 120f;
        spentTime = 0f;
        totalErrors = 0f;

        stepCount = 0;
        _cumulativeReward = 0f;
        _episodeDone = false;
        _isTouchingWall = false;
        _wallContactStreakSec = 0f;
        _isMovingBackwardFlag = 0f;
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

    private void UpdateUi()
    {
        if (speedText != null)
            speedText.text = (carController.CurrentSpeed() * 3.6).ToString("0.0");
        if (value != null)
            value.text = timer.ToString("0.00");
        if (checksGone != null)
            checksGone.text = checksOver.ToString();
        if (allChecks != null)
            allChecks.text = TrackCheckpoints.GetChecks().ToString();
        if (rewardNum != null)
            rewardNum.text = GetCumulativeReward().ToString("0.000");
    }

    public void EndEpisode()
    {
        _cumulativeReward = 0f;
        OnEpisodeBegin();
    }

    /// <summary>
    /// Возвращает вектор наблюдений для gRPC/TCP сервера (9 float).
    /// Порядок: distance_to_spline, progress, angle_to_spline, curvature,
    /// distance_to_checkpoint, direction_dot, speed, wall_contact_streak_sec,
    /// moving_backward_flag.
    /// </summary>
    public float[] GetObservationVector()
    {
        var obs = new float[9];
        obs[0] = carSplineStats.GetDistanceToSpline();
        obs[1] = carSplineStats.GetProgressAlongSpline();
        obs[2] = carSplineStats.GetAngleToSplineDirection();
        obs[3] = carSplineStats.GetLocalCurvature();

        var nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);
        obs[4] = nextCheckpoint != null ? trackCheckpoints.GetDistanceToNextCheckpoint(transform) : 0f;
        if (nextCheckpoint != null)
        {
            Vector3 dirToCheckpoint = nextCheckpoint.transform.position - transform.position;
            obs[5] = dirToCheckpoint.sqrMagnitude > 1e-6f
                ? Vector3.Dot(transform.forward, dirToCheckpoint.normalized)
                : 0f;
        }
        else
        {
            obs[5] = 0f;
        }

        obs[6] = rb.velocity.magnitude;
        obs[7] = _wallContactStreakSec;
        obs[8] = _isMovingBackwardFlag;
        return obs;
    }

    private void UpdateMovingBackwardFlag()
    {
        var nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform);
        var previousCheckpoint = trackCheckpoints.GetPreviousCheckpoint(transform);

        if (nextCheckpoint == null || previousCheckpoint == null)
        {
            _isMovingBackwardFlag = 0f;
            return;
        }

        Vector3 trackForward = (nextCheckpoint.transform.position - previousCheckpoint.transform.position).normalized;
        Vector3 velocity = rb.velocity;

        if (velocity.sqrMagnitude < 0.01f || trackForward.sqrMagnitude < 0.0001f)
        {
            _isMovingBackwardFlag = 0f;
            return;
        }

        float motionAlongTrack = Vector3.Dot(velocity.normalized, trackForward);
        _isMovingBackwardFlag = motionAlongTrack < -0.1f ? 1f : 0f;
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
        // В external control режиме нужно гарантировать, что входы применятся до Physics.Simulate().
        if (_externalControl)
            carController.Tick();
        AddReward(perStepPenalty);
        totalErrors -= perStepPenalty;

        stepCount++;
        if (stepCount >= maxSteps)
        {
            AddReward(-100f);
            totalErrors -= 100f;
            if (_externalControl)
                _episodeDone = true;
            else
                EndEpisode();
        }
    }

    /// <summary>
    /// Эпизод завершён (таймер, все чекпоинты или maxSteps).
    /// </summary>
    public bool IsEpisodeDone()
    {
        return _episodeDone || timer <= 0f || checksOver == TrackCheckpoints.GetChecks() || stepCount >= maxSteps;
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
            _isTouchingWall = true;
            AddReward(-0.1f);
            totalErrors -= 0.1f;
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            _isTouchingWall = false;
            _wallContactStreakSec = 0f;
        }
    }
}
