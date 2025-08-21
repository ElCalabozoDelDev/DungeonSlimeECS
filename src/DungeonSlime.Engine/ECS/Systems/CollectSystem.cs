using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library.Geometry;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Handles slime vs bat collection: relocate bat, randomize velocity, grow slime, update score and play SFX.
/// </summary>
public class CollectSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        // Find shared references/components used across operations
        RoomBoundsComponent? room = null;
        ScoreComponent? score = null;
        foreach (var e in world.Entities)
        {
            if (room == null && e.TryGet(out RoomBoundsComponent rb)) room = rb;
            if (score == null && e.TryGet(out ScoreComponent sc)) score = sc;
            if (room != null && score != null) break;
        }

        // Find slime and bat entities/components
        SlimeComponent? slime = null;
        SpriteComponent? slimeSprite = null;
        BatComponent? bat = null;
        TransformComponent? batTransform = null;
        SpriteComponent? batSprite = null;

        foreach (var e in world.Entities)
        {
            if (slime == null && e.TryGet(out SlimeComponent s))
            {
                slime = s;
                e.TryGet(out SpriteComponent ss);
                slimeSprite = ss;
            }

            if (bat == null && e.TryGet(out BatComponent b))
            {
                bat = b;
                e.TryGet(out TransformComponent bt);
                e.TryGet(out SpriteComponent bs);
                batTransform = bt;
                batSprite = bs;
            }

            if (slime != null && bat != null && slimeSprite != null && batSprite != null && batTransform != null)
                break;
        }

        if (slime == null || slimeSprite == null || bat == null || batSprite == null || batTransform == null)
            return;

        // Compute bounds
        var head = slime.Segments[0];
        Vector2 slimePos = Vector2.Lerp(head.At, head.To, slime.MovementProgress);
        var slimeBounds = new Circle(
            (int)(slimePos.X + slimeSprite.Sprite.Width * 0.5f),
            (int)(slimePos.Y + slimeSprite.Sprite.Height * 0.5f),
            (int)(slimeSprite.Sprite.Width * 0.5f)
        );

        var batBounds = new Circle(
            (int)(batTransform.Position.X + batSprite.Sprite.Width * 0.5f),
            (int)(batTransform.Position.Y + batSprite.Sprite.Height * 0.5f),
            (int)(batSprite.Sprite.Width * 0.25f)
        );

        // If not intersecting, nothing to do
        if (!slimeBounds.Intersects(batBounds))
            return;

        // Relocate bat away from slime within room bounds (if provided)
        if (room != null)
        {
            var r = room.Bounds;
            float roomCenterX = r.X + r.Width * 0.5f;
            float roomCenterY = r.Y + r.Height * 0.5f;
            Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);
            Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);
            Vector2 centerToSlime = slimeCenter - roomCenter;
            int padding = batBounds.Radius * 2;

            Vector2 newPos = Vector2.Zero;
            if (System.Math.Abs(centerToSlime.X) > System.Math.Abs(centerToSlime.Y))
            {
                newPos.Y = Random.Shared.Next(r.Top + padding, r.Bottom - padding);
                newPos.X = centerToSlime.X > 0 ? r.Left + padding : r.Right - padding * 2;
            }
            else
            {
                newPos.X = Random.Shared.Next(r.Left + padding, r.Right - padding);
                newPos.Y = centerToSlime.Y > 0 ? r.Top + padding : r.Bottom - padding * 2;
            }

            batTransform.Position = newPos;
        }

        // Randomize bat velocity
        BatSystem.RandomizeVelocity(bat);

        // Grow the slime by one segment
        var tail = slime.Segments[^1];
        var basePos = tail.To + tail.ReverseDirection * slime.Stride;
        var newTail = new DungeonSlime.Engine.Models.SlimeSegment
        {
            At = basePos,
            To = tail.At,
            Direction = Vector2.Normalize(tail.At - basePos)
        };
        slime.Segments.Add(newTail);

        // Update score if present
        if (score != null)
        {
            score.Score += 100;
        }

        // Play SFX if available
        // Reuse bounce SFX or use collect at scene level would need ref; skipping here to keep systems decoupled from Content
    }
}
