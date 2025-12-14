using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnPointAuthoring : MonoBehaviour
{
    class Baker : Baker<SpawnPointAuthoring>
    {
        public override void Bake(SpawnPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnPoint
            {
                IsOccupied = false,
                OccupiedByNetworkID = -1,
                Position = authoring.transform.position
            });
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawRay(transform.position, transform.up * 2);
    }
}

public struct SpawnPoint: IComponentData
{
    public bool IsOccupied;
    public int OccupiedByNetworkID;
    public float3 Position;
}