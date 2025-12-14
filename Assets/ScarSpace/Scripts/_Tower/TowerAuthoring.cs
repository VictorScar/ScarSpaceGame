using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TowerAuthoring : MonoBehaviour
{
  public int DefaultFractionID = 0;
  
  public class TowerBacker: Baker<TowerAuthoring>
  {
    public override void Bake(TowerAuthoring authoring)
    {
      var entity = GetEntity(TransformUsageFlags.Dynamic);
      
      AddComponent(entity, new FractionID
      {
        Value = authoring.DefaultFractionID
      });
    }
  }
}
