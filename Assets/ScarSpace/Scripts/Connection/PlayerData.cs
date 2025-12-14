using Unity.Entities;

public struct PlayerData : IComponentData
{
    public int ColorID;
    public int TeamID; // На будущее (союзники)
}