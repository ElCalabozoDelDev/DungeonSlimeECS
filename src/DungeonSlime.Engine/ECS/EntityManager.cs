using System;
using System.Collections.Generic;

namespace DungeonSlime.Engine.ECS;

/// <summary>
/// Manages entity lifecycle and stores a list of active entities.
/// </summary>
public class EntityManager
{
    private int _nextId = 1;
    private readonly List<Entity> _entities = new();

    public IReadOnlyList<Entity> Entities => _entities;

    public Entity Create()
    {
        var e = new Entity(_nextId++);
        _entities.Add(e);
        return e;
    }

    public void Destroy(Entity entity)
    {
        _entities.Remove(entity);
    }
}
