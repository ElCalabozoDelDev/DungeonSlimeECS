using System;
using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.Input;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Handles input, movement tick, growth and self-collision for the slime using ECS components.
/// </summary>
public class SlimeSystem : IEcsSystem
{
    private static readonly TimeSpan s_movementTime = TimeSpan.FromMilliseconds(200);

    public void Update(GameTime gameTime, EntityManager world)
    {
        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out SlimeComponent slime) || !e.TryGet(out TransformComponent transform) || !e.TryGet(out SpriteComponent sprite))
                continue;

            // Update sprite animation
            sprite.Sprite.Update(gameTime);

            // Handle input buffering
            HandleInput(slime);

            // Tick movement timer
            slime.MovementTimer += gameTime.ElapsedGameTime;
            if (slime.MovementTimer >= s_movementTime)
            {
                slime.MovementTimer -= s_movementTime;
                Move(slime);
            }

            // Update interpolation progress
            slime.MovementProgress = (float)(slime.MovementTimer.TotalSeconds / s_movementTime.TotalSeconds);
        }
    }

    private static void HandleInput(SlimeComponent slime)
    {
        var potential = Vector2.Zero;
        if (GameController.MoveUp()) potential = -Vector2.UnitY;
        else if (GameController.MoveDown()) potential = Vector2.UnitY;
        else if (GameController.MoveLeft()) potential = -Vector2.UnitX;
        else if (GameController.MoveRight()) potential = Vector2.UnitX;

        if (potential != Vector2.Zero && slime.InputBuffer.Count < 2)
        {
            var validateAgainst = slime.InputBuffer.Count > 0 ? slime.InputBuffer.Peek() : (slime.Segments.Count > 0 ? slime.Segments[0].Direction : Vector2.UnitX);
            if (Vector2.Dot(potential, validateAgainst) >= 0)
            {
                slime.InputBuffer.Enqueue(potential);
            }
        }
    }

    private static void Move(SlimeComponent slime)
    {
        if (slime.InputBuffer.Count > 0)
        {
            slime.NextDirection = slime.InputBuffer.Dequeue();
        }

        var head = slime.Segments[0];
        head.Direction = slime.NextDirection;
        head.At = head.To;
        head.To = head.At + head.Direction * slime.Stride;

        slime.Segments.Insert(0, head);
        slime.Segments.RemoveAt(slime.Segments.Count - 1);

        // Self-collision check
        for (int i = 1; i < slime.Segments.Count; i++)
        {
            if (slime.Segments[0].At == slime.Segments[i].At)
            {
                slime.BodyCollisionDetected = true;
                return;
            }
        }
    }
}
