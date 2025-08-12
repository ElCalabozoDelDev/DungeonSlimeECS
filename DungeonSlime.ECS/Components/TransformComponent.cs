using Microsoft.Xna.Framework;

namespace DungeonSlime.ECS.Components;

public class TransformComponent
{
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; } = Vector2.Zero;
}