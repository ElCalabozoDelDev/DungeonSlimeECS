namespace DungeonSlime.Engine.ECS.Components;

/// <summary>
/// Shared game state component used by systems to signal high-level state changes
/// like GameOver without directly coupling to the scene.
/// </summary>
public class GameStateComponent
{
    public bool IsGameOver { get; set; }
}
