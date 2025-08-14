using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Updates bat movement and handles wall bounces.
/// </summary>
public class BatSystem
{
    public void Update(GameTime gameTime, EntityManager world, Rectangle roomBounds)
    {
        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out BatComponent bat) || !e.TryGet(out TransformComponent transform) || !e.TryGet(out SpriteComponent sprite))
                continue;

            // Animate
            sprite.Sprite.Update(gameTime);

            // Move
            transform.Position += bat.Velocity;

            // Bounds for collision test
            var x = (int)(transform.Position.X + sprite.Sprite.Width * 0.5f);
            var y = (int)(transform.Position.Y + sprite.Sprite.Height * 0.5f);
            var radius = (int)(sprite.Sprite.Width * 0.25f);
            var bounds = new Circle(x, y, radius);

            // Bounce off walls
            if (bounds.Top < roomBounds.Top)
            {
                Bounce(transform, bat, Vector2.UnitY);
            }
            else if (bounds.Bottom > roomBounds.Bottom)
            {
                Bounce(transform, bat, -Vector2.UnitY);
            }

            if (bounds.Left < roomBounds.Left)
            {
                Bounce(transform, bat, Vector2.UnitX);
            }
            else if (bounds.Right > roomBounds.Right)
            {
                Bounce(transform, bat, -Vector2.UnitX);
            }
        }
    }

    private static void Bounce(TransformComponent transform, BatComponent bat, Vector2 normal)
    {
        // Nudge away from wall
        if (normal.X != 0)
            transform.Position.X += normal.X * 4f; // small nudge
        if (normal.Y != 0)
            transform.Position.Y += normal.Y * 4f;

        // Reflect velocity
        bat.Velocity = Vector2.Reflect(bat.Velocity, normal);

        // Play SFX
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
