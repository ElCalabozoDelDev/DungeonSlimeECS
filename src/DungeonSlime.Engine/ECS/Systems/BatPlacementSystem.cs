using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library.Geometry;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Places the bat away from the slime on start, then disables itself by clearing the flag on the BatComponent.
/// </summary>
public class BatPlacementSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        RoomBoundsComponent? room = null;
        SlimeComponent? slime = null;
        SpriteComponent? slimeSprite = null;

        foreach (var e in world.Entities)
        {
            if (room == null && e.TryGet(out RoomBoundsComponent rb)) room = rb;
            if (slime == null && e.TryGet(out SlimeComponent sc))
            {
                slime = sc;
                e.TryGet(out SpriteComponent ss);
                slimeSprite = ss;
            }
            if (room != null && slime != null && slimeSprite != null) break;
        }

        if (room == null || slime == null || slimeSprite == null)
            return;

        // Compute slime head circle
        var head = slime.Segments[0];
        Vector2 slimePos = Vector2.Lerp(head.At, head.To, slime.MovementProgress);
        var slimeBounds = new Circle(
            (int)(slimePos.X + slimeSprite.Sprite.Width * 0.5f),
            (int)(slimePos.Y + slimeSprite.Sprite.Height * 0.5f),
            (int)(slimeSprite.Sprite.Width * 0.5f)
        );

        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out BatComponent bat) || !e.TryGet(out TransformComponent batTransform) || !e.TryGet(out SpriteComponent batSprite))
                continue;

            if (!bat.NeedsInitialPlacement)
                continue;

            var r = room.Bounds;
            float roomCenterX = r.X + r.Width * 0.5f;
            float roomCenterY = r.Y + r.Height * 0.5f;
            Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);
            Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);
            Vector2 centerToSlime = slimeCenter - roomCenter;
            var batBoundsRadius = (int)(batSprite.Sprite.Width * 0.25f);
            int padding = batBoundsRadius * 2;

            Vector2 newBatPosition = Vector2.Zero;
            if (System.Math.Abs(centerToSlime.X) > System.Math.Abs(centerToSlime.Y))
            {
                newBatPosition.Y = Random.Shared.Next(r.Top + padding, r.Bottom - padding);
                newBatPosition.X = centerToSlime.X > 0 ? r.Left + padding : r.Right - padding * 2;
            }
            else
            {
                newBatPosition.X = Random.Shared.Next(r.Left + padding, r.Right - padding);
                newBatPosition.Y = centerToSlime.Y > 0 ? r.Top + padding : r.Bottom - padding * 2;
            }

            batTransform.Position = newBatPosition;
            bat.NeedsInitialPlacement = false;
        }
    }
}
