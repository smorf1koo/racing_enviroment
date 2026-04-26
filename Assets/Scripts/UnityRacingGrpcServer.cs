using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;
using UnityRacing;

/// <summary>
/// gRPC-сервер для RL-клиента (Python). Реализует UnityRacingService.
/// Настройка: добавь на GameObject, укажи CarControllerAgent. Порт 50051.
/// Требуется: Grpc.Core и Google.Protobuf (через NuGet For Unity).
/// </summary>
[AddComponentMenu("Racing/Unity Racing gRPC Server")]
public class UnityRacingGrpcServer : MonoBehaviour
{
    [SerializeField] private CarControllerAgent agent;
    [SerializeField] private int port = 50051;

    private Server _server;
    private float _lastCumulativeReward;
    private const int FixedFramesPerStep = 4;

    private void Awake()
    {
        if (agent == null)
            agent = FindObjectOfType<CarControllerAgent>();
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("[UnityRacingGrpcServer] CarControllerAgent not found!");
            return;
        }

        UnityMainThreadDispatcher.Instance();

        var impl = new RacingServiceImpl(agent, this);
        _server = new Server
        {
            Services = { UnityRacing.UnityRacingService.BindService(impl) },
            Ports = { new ServerPort("0.0.0.0", port, ServerCredentials.Insecure) }
        };
        _server.Start();

        _lastCumulativeReward = agent.GetCumulativeReward();
        agent.SetExternalControl(true);
        Debug.Log($"[UnityRacingGrpcServer] gRPC server listening on port {port}");
    }

    private void OnDestroy()
    {
        _server?.ShutdownAsync().Wait();
    }

    internal void SetLastCumulativeReward(float value) => _lastCumulativeReward = value;
    internal float GetLastCumulativeReward() => _lastCumulativeReward;

    private sealed class RacingServiceImpl : UnityRacing.UnityRacingService.UnityRacingServiceBase
    {
        private readonly CarControllerAgent _agent;
        private readonly UnityRacingGrpcServer _owner;

        public RacingServiceImpl(CarControllerAgent agent, UnityRacingGrpcServer owner)
        {
            _agent = agent;
            _owner = owner;
        }

        public override Task<ResetResponse> Reset(ResetRequest request, ServerCallContext context)
        {
            var response = UnityMainThreadDispatcher.Instance().EnqueueAndWait(() =>
            {
                UnityEngine.Random.InitState(request.Seed);
                _agent.EndEpisode();
                _agent.OnEpisodeBegin();
                _owner.SetLastCumulativeReward(_agent.GetCumulativeReward());

                var obs = _agent.GetObservationVector();
                var resp = new ResetResponse();
                resp.Observation.Add(obs);
                return resp;
            });
            return Task.FromResult(response);
        }

        public override Task<StepResponse> Step(StepRequest request, ServerCallContext context)
        {
            float forward = 0f, turn = 0f;
            if (request.Action.Count >= 2)
            {
                forward = Mathf.Clamp(request.Action[0], -1f, 1f);
                turn = Mathf.Clamp(request.Action[1], -1f, 1f);
            }

            var fwd = forward;
            var trn = turn;
            var result = UnityMainThreadDispatcher.Instance().EnqueueAndWait(() =>
            {
                _agent.ApplyAction(fwd, trn);
                for (int i = 0; i < FixedFramesPerStep; i++)
                    Physics.Simulate(Time.fixedDeltaTime);

                float current = _agent.GetCumulativeReward();
                float stepReward = current - _owner.GetLastCumulativeReward();
                _owner.SetLastCumulativeReward(current);

                var resp = new StepResponse
                {
                    Reward = stepReward,
                    Terminated = _agent.IsEpisodeDone(),
                    Truncated = false
                };
                resp.Observation.Add(_agent.GetObservationVector());
                return resp;
            });
            return Task.FromResult(result);
        }
    }
}
