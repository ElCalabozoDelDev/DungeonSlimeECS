using DungeonSlime.Engine.Models;
using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Components;

public class SlimeComponent
{
    public List<SlimeSegment> Segments { get; set; } = new();
    public float Stride { get; set; }
    public float MovementProgress { get; set; }
    public Vector2 NextDirection { get; set; }
    public Queue<Vector2> InputBuffer { get; set; } = new(2);
    public TimeSpan MovementTimer { get; set; } = TimeSpan.Zero;
}