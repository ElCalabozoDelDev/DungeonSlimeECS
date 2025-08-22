using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Components;

/// <summary>
/// Component for entities that can move with velocity and physics.
/// </summary>
public class VelocityComponent
{
    public Vector2 Velocity { get; set; }
    public float MaxSpeed { get; set; }
    public float Acceleration { get; set; } = 1.0f;
    public float Friction { get; set; } = 0.98f;
}