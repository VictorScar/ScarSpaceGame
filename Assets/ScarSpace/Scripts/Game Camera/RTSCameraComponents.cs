using Unity.Entities;
using Unity.Mathematics;

public struct RTSCameraConfig : IComponentData
{
    public float MoveSpeed;
    public float ZoomSpeed;
    public float Height;
}

public struct ActiveCameraTag : IComponentData
{
}

public struct CameraInput : IComponentData
{
    public float2 MoveInput;
    public float ZoomInput;
}