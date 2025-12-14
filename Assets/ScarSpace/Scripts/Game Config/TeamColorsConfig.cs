using UnityEngine;

[CreateAssetMenu(fileName = "TeamColors", menuName = "Game/TeamColors")]
public class TeamColorsConfig : ScriptableObject
{
    public Color[] Colors = new Color[] 
    { 
        Color.red,      // Игрок 0 (или 1)
        Color.blue,     // Игрок 1
        Color.green,    // Игрок 2
        Color.yellow    // Игрок 3
    };
}

