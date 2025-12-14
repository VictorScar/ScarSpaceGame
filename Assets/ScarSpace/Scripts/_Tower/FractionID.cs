using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct FractionID : IComponentData
{
    [GhostField] public int Value;
}
