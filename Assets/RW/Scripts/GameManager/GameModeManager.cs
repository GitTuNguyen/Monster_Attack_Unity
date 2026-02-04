using UnityEngine;

[System.Serializable]
public enum GameMode
{
    Offline,
    Multiplayer
}

public static class GameModeManager
{
    public static GameMode Mode = GameMode.Offline;
}

