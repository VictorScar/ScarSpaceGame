using Unity.Entities;

public struct ServerGameState : IComponentData
{
    public int NextColorIndex; // Какой цвет выдать следующему? (0, 1, 2...)
}