using Unity.InferenceEngine;
using UnityEngine;

/// <summary>
/// Контроллер инференса ONNX-модели для автономного управления машиной.
/// Загружает обученную модель, собирает observations через CarControllerAgent
/// и подаёт actions каждый FixedUpdate.
///
/// Использование:
///   1. Добавь этот компонент на машину (рядом с CarControllerAgent)
///   2. В Inspector перетащи .onnx файл в поле Model Asset
///   3. Нажми Play — машина поедет под управлением модели
/// </summary>
[RequireComponent(typeof(CarControllerAgent))]
[AddComponentMenu("Racing/ONNX Inference Controller")]
public class OnnxInferenceController : MonoBehaviour
{
    [Header("Model")]
    [Tooltip("ONNX-модель, обученная через claw-engine-rl")]
    [SerializeField] private ModelAsset modelAsset;

    [Header("Inference")]
    [Tooltip("CPU или GPU")]
    [SerializeField] private BackendType backend = BackendType.GPUCompute;

    [Tooltip("Имя входного тензора в ONNX")]
    [SerializeField] private string inputName = "obs";

    [Tooltip("Имя выходного тензора в ONNX")]
    [SerializeField] private string outputName = "actions";

    [Header("Debug")]
    [SerializeField] private bool logActions = false;

    private CarControllerAgent _agent;
    private Model _model;
    private Worker _worker;
    private bool _ready;

    private void Awake()
    {
        _agent = GetComponent<CarControllerAgent>();
    }

    private void Start()
    {
        if (modelAsset == null)
        {
            Debug.LogError("[OnnxInference] Model Asset не назначен!", this);
            return;
        }

        _model = ModelLoader.Load(modelAsset);

        if (_model.inputs.Count > 0)
        {
            string detectedInput = _model.inputs[0].name;
            if (inputName != detectedInput)
            {
                Debug.Log($"[OnnxInference] Вход модели: '{detectedInput}'. Используем его.");
                inputName = detectedInput;
            }
        }

        if (_model.outputs.Count > 0)
        {
            string detectedOutput = _model.outputs[0].name;
            if (outputName != detectedOutput)
            {
                Debug.Log($"[OnnxInference] Выход модели: '{detectedOutput}'. Используем его.");
                outputName = detectedOutput;
            }
        }

        _worker = new Worker(_model, backend);
        _agent.SetExternalControl(true);
        _ready = true;

        Debug.Log($"[OnnxInference] Модель загружена. Вход: '{inputName}', Выход: '{outputName}', Backend: {backend}");
    }

    private void FixedUpdate()
    {
        if (!_ready || _agent.IsEpisodeDone()) return;

        float[] obs = _agent.GetObservationVector();
        if (obs == null || obs.Length == 0) return;

        using var inputTensor = new Tensor<float>(new TensorShape(1, obs.Length), obs);

        _worker.SetInput(inputName, inputTensor);
        _worker.Schedule();

        var outputTensor = _worker.PeekOutput(outputName) as Tensor<float>;
        if (outputTensor == null) return;

        outputTensor.ReadbackAndClone();

        float forward = Mathf.Clamp(outputTensor[0, 0], -1f, 1f);
        float turn = Mathf.Clamp(outputTensor[0, 1], -1f, 1f);

        _agent.ApplyAction(forward, turn);

        if (logActions)
        {
            Debug.Log($"[OnnxInference] forward={forward:F3} turn={turn:F3}");
        }
    }

    private void OnDestroy()
    {
        _worker?.Dispose();
    }
}
