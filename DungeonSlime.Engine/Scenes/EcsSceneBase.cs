using Microsoft.Xna.Framework;
using DungeonSlime.Library.Scenes;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DungeonSlime.Engine.Scenes;

/// <summary>
/// Base scene providing a reusable ECS world and a simple system update loop.
/// </summary>
public abstract class EcsSceneBase : Scene
{
    protected EntityManager World { get; private set; } = new EntityManager();
    private readonly List<IEcsSystem> _systems = new();

    /// <summary>
    /// Registers a system to be updated each frame in insertion order.
    /// </summary>
    protected void RegisterSystem(IEcsSystem system)
    {
        _systems.Add(system);
    }

    /// <summary>
    /// Clears systems and resets the ECS world.
    /// </summary>
    protected void ResetWorld()
    {
        World = new EntityManager();
        _systems.Clear();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Update registered systems
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(gameTime, World);
        }
    }
}
