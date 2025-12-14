using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class RTSCameraSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = SystemAPI.Time.DeltaTime;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float scroll = Input.mouseScrollDelta.y;

        foreach (var (transform, config) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRW<RTSCameraConfig>>().WithAll<ActiveCameraTag>())
        {
            var cfg = config.ValueRO;

            float3 move = new float3(h, 0, v) * cfg.MoveSpeed * dt;
            transform.ValueRW.Position += move;

            config.ValueRW.Height -= scroll * cfg.ZoomSpeed;
            config.ValueRW.Height = math.clamp(config.ValueRW.Height, 5f, 50f);

            transform.ValueRW.Position.y = config.ValueRW.Height;
            transform.ValueRW.Rotation = quaternion.RotateX(math.radians(60));
        }
    }
}