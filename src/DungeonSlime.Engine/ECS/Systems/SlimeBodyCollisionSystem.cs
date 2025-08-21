using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Converts slime self-collision detection into a shared game over signal via GameStateComponent.
/// Keeps scene logic decoupled from ECS events.
/// </summary>
public class SlimeBodyCollisionSystem : IUpdateSystem
{
    public void Update(GameTime gameTime, EntityManager world)
    {
        SlimeComponent? slime = null;
        GameStateComponent? gameState = null;

        // Gather required components from the world.
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
            gameState.IsGameOver = true;
        }
    }
}
