using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Components;

/// <summary>
/// Component for entities that have collision bounds.
/// </summary>
public class ColliderComponent
{
    public Rectangle Bounds { get; set; }
    public bool IsTrigger { get; set; } = false;
    public string CollisionLayer { get; set; } = "Default";
}