using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using DungeonSlime.Library.Graphics;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.Models;

namespace DungeonSlime.Engine.ECS;

/// <summary>
/// Factory for creating game entities with their required components.
/// </summary>
public static class GameEntityFactory
{
    public static Entity CreateSlime(EntityManager world, Vector2 position, float stride, AnimatedSprite sprite)
    {
        var entity = world.Create();
        var transform = new TransformComponent { Position = position, Direction = Vector2.UnitX };
        var spriteComponent = new SpriteComponent { Sprite = sprite };
        var slime = new SlimeComponent
        {
            Stride = (int)stride,
            NextDirection = Vector2.UnitX,
            MovementTimer = TimeSpan.Zero,
            MovementProgress = 0f
        };

        var head = new SlimeSegment 
        { 
            At = position, 
            To = position + new Vector2(stride, 0), 
            Direction = Vector2.UnitX 
        };
        slime.Segments.Add(head);

        entity.Add(transform);
        entity.Add(spriteComponent);
        entity.Add(slime);

        return entity;
    }

    public static Entity CreateBat(EntityManager world, AnimatedSprite sprite, SoundEffect? bounceSfx = null)
    {
        var entity = world.Create();
        var transform = new TransformComponent { Position = Vector2.Zero };
        var spriteComponent = new SpriteComponent { Sprite = sprite };
        var bat = new BatComponent 
        { 
            MovementSpeed = 5.0f, 
            BounceSoundEffect = bounceSfx, 
            NeedsInitialPlacement = true 
        };

        entity.Add(transform);
        entity.Add(spriteComponent);
        entity.Add(bat);

        return entity;
    }

    public static Entity CreateRoomBounds(EntityManager world, Rectangle bounds)
    {
        var entity = world.Create();
        entity.Add(new RoomBoundsComponent { Bounds = bounds });
        return entity;
    }

    public static Entity CreateScore(EntityManager world, int initialScore = 0)
    {
        var entity = world.Create();
        entity.Add(new ScoreComponent { Score = initialScore });
        return entity;
    }

    public static Entity CreateGameState(EntityManager world, GameState initialState = GameState.Playing)
    {
        var entity = world.Create();
        entity.Add(new GameStateComponent { State = initialState });
        return entity;
    }

    public static Entity CreateGameAudio(EntityManager world, SoundEffect collectSfx)
    {
        var entity = world.Create();
        entity.Add(new GameAudioComponent { CollectSoundEffect = collectSfx });
        return entity;
    }
}