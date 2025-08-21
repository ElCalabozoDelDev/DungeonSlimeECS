using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library;
using DungeonSlime.Library.Geometry;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Updates bat movement and handles wall bounces.
/// </summary>
public class BatSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        // Obtain shared room bounds if available
        Rectangle? roomBounds = null;
        foreach (var e in world.Entities)
        {
            if (e.TryGet(out RoomBoundsComponent rb))
            {
                roomBounds = rb.Bounds;
                break;
            }
        }

        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out BatComponent bat) || !e.TryGet(out TransformComponent transform) || !e.TryGet(out SpriteComponent sprite))
                continue;

            // Animate
            sprite.Sprite.Update(gameTime);

            // Move
            transform.Position += bat.Velocity;

            if (roomBounds.HasValue)
            {
                var r = roomBounds.Value;
                // Bounds for collision test
                var x = (int)(transform.Position.X + sprite.Sprite.Width * 0.5f);
                var y = (int)(transform.Position.Y + sprite.Sprite.Height * 0.5f);
                var radius = (int)(sprite.Sprite.Width * 0.25f);
                var bounds = new Circle(x, y, radius);

                // Bounce off walls
                if (bounds.Top < r.Top)
                {
                    Bounce(transform, bat, Vector2.UnitY);
                }
                else if (bounds.Bottom > r.Bottom)
                {
                    Bounce(transform, bat, -Vector2.UnitY);
                }

                if (bounds.Left < r.Left)
                {
                    Bounce(transform, bat, Vector2.UnitX);
                }
                else if (bounds.Right > r.Right)
                {
                    Bounce(transform, bat, -Vector2.UnitX);
                }
            }
        }
    }

    private static void Bounce(TransformComponent transform, BatComponent bat, Vector2 normal)
    {
        var position = transform.Position;
        if (normal.X != 0)
            position.X += normal.X * 4f;
        if (normal.Y != 0)
            position.Y += normal.Y * 4f;
        transform.Position = position;

        bat.Velocity = Vector2.Reflect(bat.Velocity, normal);

        if (bat.BounceSoundEffect != null)
            Core.Audio.PlaySoundEffect(bat.BounceSoundEffect);
    }

    public static void RandomizeVelocity(BatComponent bat)
    {
        float angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        var dir = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
        bat.Velocity = dir * bat.MovementSpeed;
    }
}
