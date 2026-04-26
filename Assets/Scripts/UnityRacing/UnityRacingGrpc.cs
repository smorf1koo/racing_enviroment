// Hand-written gRPC stub compatible with Grpc.Core 2.46.6 (NuGet).
#pragma warning disable 0414, 1591, 8981, 0612
#region Designer generated code

using grpc = global::Grpc.Core;

namespace UnityRacing {
  /// <summary>
  /// Сервис для RL-окружения гонок
  /// </summary>
  public static partial class UnityRacingService
  {
    static readonly string __ServiceName = "unity_racing.UnityRacingService";

    static readonly grpc::Marshaller<global::UnityRacing.ResetRequest> __Marshaller_unity_racing_ResetRequest =
        grpc::Marshallers.Create(
            (arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg),
            global::UnityRacing.ResetRequest.Parser.ParseFrom);

    static readonly grpc::Marshaller<global::UnityRacing.ResetResponse> __Marshaller_unity_racing_ResetResponse =
        grpc::Marshallers.Create(
            (arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg),
            global::UnityRacing.ResetResponse.Parser.ParseFrom);

    static readonly grpc::Marshaller<global::UnityRacing.StepRequest> __Marshaller_unity_racing_StepRequest =
        grpc::Marshallers.Create(
            (arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg),
            global::UnityRacing.StepRequest.Parser.ParseFrom);

    static readonly grpc::Marshaller<global::UnityRacing.StepResponse> __Marshaller_unity_racing_StepResponse =
        grpc::Marshallers.Create(
            (arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg),
            global::UnityRacing.StepResponse.Parser.ParseFrom);

    static readonly grpc::Method<global::UnityRacing.ResetRequest, global::UnityRacing.ResetResponse> __Method_Reset =
        new grpc::Method<global::UnityRacing.ResetRequest, global::UnityRacing.ResetResponse>(
            grpc::MethodType.Unary,
            __ServiceName,
            "Reset",
            __Marshaller_unity_racing_ResetRequest,
            __Marshaller_unity_racing_ResetResponse);

    static readonly grpc::Method<global::UnityRacing.StepRequest, global::UnityRacing.StepResponse> __Method_Step =
        new grpc::Method<global::UnityRacing.StepRequest, global::UnityRacing.StepResponse>(
            grpc::MethodType.Unary,
            __ServiceName,
            "Step",
            __Marshaller_unity_racing_StepRequest,
            __Marshaller_unity_racing_StepResponse);

    /// <summary>Base class for server-side implementations of UnityRacingService</summary>
    public abstract partial class UnityRacingServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::UnityRacing.ResetResponse> Reset(
          global::UnityRacing.ResetRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::UnityRacing.StepResponse> Step(
          global::UnityRacing.StepRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    public static grpc::ServerServiceDefinition BindService(UnityRacingServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Reset, serviceImpl.Reset)
          .AddMethod(__Method_Step, serviceImpl.Step)
          .Build();
    }
  }
}
#endregion
