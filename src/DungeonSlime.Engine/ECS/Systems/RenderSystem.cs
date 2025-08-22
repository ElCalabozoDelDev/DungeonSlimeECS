using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Unified rendering system that handles all sprite rendering including special slime segment rendering.
/// </summary>
public class RenderSystem : IRenderSystem
{
    public void Draw(GameTime gameTime, EntityManager world)
    {
        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out SpriteComponent sprite) || !e.TryGet(out TransformComponent transform))
                continue;

            // Special handling for slime entities (render all segments)
            if (e.TryGet(out SlimeComponent slime))
            {
                DrawSlimeSegments(slime, sprite);
            }
            else
            {
                // Regular sprite rendering for non-slime entities
                sprite.Sprite.Draw(Core.SpriteBatch, transform.Position);
            }
        }
    }

    private void DrawSlimeSegments(SlimeComponent slime, SpriteComponent sprite)
    {
        for (int i = 0; i < slime.Segments.Count; i++)
        {
            var seg = slime.Segments[i];
            var pos = Vector2.Lerp(seg.At, seg.To, slime.MovementProgress);
            sprite.Sprite.Draw(Core.SpriteBatch, pos);
        }
    }
}