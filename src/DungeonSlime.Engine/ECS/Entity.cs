using System;
using System.Collections.Generic;

namespace DungeonSlime.Engine.ECS;

/// <summary>
/// Basic entity which holds a bag of components indexed by type.
/// </summary>
public class Entity
{
    private readonly Dictionary<Type, object> _components = new();

    public int Id { get; }

    internal Entity(int id)
    {
        Id = id;
    }

    public void Add<T>(T component) where T : class
    {
        _components[typeof(T)] = component;
    }

    public bool Has<T>() where T : class
    {
        return _components.ContainsKey(typeof(T));
    }

    public T Get<T>() where T : class
    {
        return (T)_components[typeof(T)];
    }

    public bool TryGet<T>(out T component) where T : class
    {
        if (_components.TryGetValue(typeof(T), out var obj) && obj is T typed)
        {
            component = typed;
            return true;
        }

        component = null!;
        return false;
    }
}
