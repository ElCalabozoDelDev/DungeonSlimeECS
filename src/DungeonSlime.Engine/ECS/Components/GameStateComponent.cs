namespace DungeonSlime.Engine.ECS.Components;

/// <summary>
/// Shared game state component used by systems to signal high-level state changes
/// like GameOver without directly coupling to the scene.
/// </summary>
public class GameStateComponent
{
    public DungeonSlime.Engine.Scenes.GameState State { get; set; } = DungeonSlime.Engine.Scenes.GameState.Playing;
}
