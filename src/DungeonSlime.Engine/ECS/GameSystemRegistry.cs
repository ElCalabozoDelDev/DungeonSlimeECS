using System.Collections.Generic;
using DungeonSlime.Engine.Contracts;
using DungeonSlime.Engine.ECS.Systems;

namespace DungeonSlime.Engine.ECS;

/// <summary>
/// Registry that manages system creation and registration for game scenes.
/// </summary>
public class GameSystemRegistry
{
    private readonly List<IUpdateSystem> _updateSystems = new();
    private readonly List<IRenderSystem> _renderSystems = new();

    public void RegisterGameplaySystems()
    {
        // Order matters for update systems
        _updateSystems.Add(new SlimeSystem()); // First: Move slime and detect body collision
        _updateSystems.Add(new BatSystem()); // Second: Move bat
        _updateSystems.Add(new CollectSystem()); // Third: Handle slime-bat collision
        _updateSystems.Add(new CollisionSystem()); // Fourth: Handle all collision detection (unified)
        _updateSystems.Add(new UiNotificationSystem()); // Fifth: Detect state changes and notify UI
        _updateSystems.Add(new BatPlacementSystem()); // Last: Handle bat placement (less critical)
    }

    public void RegisterRenderSystems()
    {
        _renderSystems.Add(new RenderSystem()); // Single unified render system
    }

    public IEnumerable<IUpdateSystem> UpdateSystems => _updateSystems;
    public IEnumerable<IRenderSystem> RenderSystems => _renderSystems;

    public void Clear()
    {
        _updateSystems.Clear();
        _renderSystems.Clear();
    }

    // Get specific systems for external access
    public T? GetSystem<T>() where T : class
    {
        var updateSystem = _updateSystems.OfType<T>().FirstOrDefault();
        if (updateSystem != null) return updateSystem;

        return _renderSystems.OfType<T>().FirstOrDefault();
    }
}