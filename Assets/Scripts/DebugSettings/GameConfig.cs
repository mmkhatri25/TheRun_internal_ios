using UnityEngine;

public enum GameType
{
    Debug,
    Release
}

[CreateAssetMenu(fileName = "BuildType", menuName = "Game/BuildType")]
public class GameConfig : ScriptableObject
{
    public GameType gameType;
}
