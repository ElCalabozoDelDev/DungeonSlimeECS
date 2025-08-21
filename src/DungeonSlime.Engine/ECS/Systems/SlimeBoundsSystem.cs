using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Detects when the slime goes out of the room bounds and flags game over via GameStateComponent.
/// </summary>
public class SlimeBoundsSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        RoomBoundsComponent? room = null;
        GameStateComponent? state = null;
        SlimeComponent? slime = null;
        SpriteComponent? slimeSprite = null;

        foreach (var e in world.Entities)
        {
            if (room == null && e.TryGet(out RoomBoundsComponent rb)) room = rb;
            if (state == null && e.TryGet(out GameStateComponent gs)) state = gs;
            if (slime == null && e.TryGet(out SlimeComponent sc))
            {
                slime = sc;
                e.TryGet(out SpriteComponent ss);
                slimeSprite = ss;
            }

            if (room != null && state != null && slime != null && slimeSprite != null)
                break;
        }

        if (room == null || state == null || slime == null || slimeSprite == null)
            return;

        // Compute current slime head circle
        var head = slime.Segments[0];
        var pos = Vector2.Lerp(head.At, head.To, slime.MovementProgress);
        var radius = (int)(slimeSprite.Sprite.Width * 0.5f);
        var circleX = (int)(pos.X + radius);
        var circleY = (int)(pos.Y + radius);

        var bounds = room.Bounds;
        if (circleY - radius < bounds.Top ||
            circleY + radius > bounds.Bottom ||
            circleX - radius < bounds.Left ||
            circleX + radius > bounds.Right)
        {
            state.State = GameState.GameOver;
        }
    }
}
