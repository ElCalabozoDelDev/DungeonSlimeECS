using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Unified collision system that handles both room bounds collision and slime self-collision.
/// </summary>
public class CollisionSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        CheckSlimeBoundsCollision(world);
        CheckSlimeBodyCollision(world);
    }

    private void CheckSlimeBoundsCollision(EntityManager world)
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

    private void CheckSlimeBodyCollision(EntityManager world)
    {
        SlimeComponent? slime = null;
        GameStateComponent? gameState = null;

        foreach (var e in world.Entities)
        {
            if (slime is null && e.TryGet(out SlimeComponent s))
            {
                slime = s;
            }

            if (gameState is null && e.TryGet(out GameStateComponent gs))
            {
                gameState = gs;
            }

            if (slime is not null && gameState is not null)
                break;
        }

        if (slime is null || gameState is null)
            return;

        // If a body collision was detected by the slime system, flag game over and reset the flag.
        if (slime.BodyCollisionDetected)
        {
            slime.BodyCollisionDetected = false;
            gameState.State = GameState.GameOver;
        }
    }
}