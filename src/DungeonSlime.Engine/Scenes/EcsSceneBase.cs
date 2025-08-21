using Microsoft.Xna.Framework;
using DungeonSlime.Library.Scenes;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Systems;
using System.Collections.Generic;

namespace DungeonSlime.Engine.Scenes;

/// <summary>
/// Base scene providing a reusable ECS world and simple system loops for update and draw.
/// </summary>
public abstract class EcsSceneBase : Scene
{
    protected EntityManager World { get; private set; } = new EntityManager();
    private readonly List<IUpdateSystem> _systems = new();
    private readonly List<IRenderSystem> _renderSystems = new();

    /// <summary>
    /// Registers an update system to be updated each frame in insertion order.
    /// </summary>
    protected void RegisterSystem(IUpdateSystem system)
    {
        _systems.Add(system);
    }

    /// <summary>
    /// Registers a render system to be drawn each frame in insertion order.
    /// </summary>
    protected void RegisterRenderSystem(IRenderSystem system)
    {
        _renderSystems.Add(system);
    }

    /// <summary>
    /// Clears systems and resets the ECS world.
    /// </summary>
    protected void ResetWorld()
    {
        World = new EntityManager();
        _systems.Clear();
        _renderSystems.Clear();
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

    public override void Draw(GameTime gameTime)
    {
        // Do not call base.Draw here; Scene.Draw is empty by default.
        for (int i = 0; i < _renderSystems.Count; i++)
        {
            _renderSystems[i].Draw(gameTime, World);
        }
    }
}
