namespace DungeonSlime.Engine.ECS;

/// <summary>
/// High-level game state values used by GameScene and related systems.
/// Kept in the Scenes namespace and alongside GameScene for proximity.
/// </summary>
public enum GameState
{
    Playing,
    Paused,
    GameOver
}
