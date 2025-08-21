using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Renders all entities having SpriteComponent and TransformComponent except slime entities (handled by Slime render logic).
/// Assumes SpriteBatch has already begun in the scene.
/// </summary>
public class RenderSystem : IRenderSystem
{
    public void Draw(GameTime gameTime, EntityManager world)
    {
        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out SpriteComponent sprite) || !e.TryGet(out TransformComponent transform))
                continue;

            // Skip slime entities; they are drawn by SlimeRenderSystem (segments)
            if (e.Has<SlimeComponent>())
                continue;

            sprite.Sprite.Draw(Core.SpriteBatch, transform.Position);
        }
    }
}
