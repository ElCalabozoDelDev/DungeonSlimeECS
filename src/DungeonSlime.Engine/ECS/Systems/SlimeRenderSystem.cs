using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Library;
using DungeonSlime.Engine.Contracts;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Renders the slime as a chain of segments using its AnimatedSprite and movement interpolation.
/// </summary>
public class SlimeRenderSystem : IRenderSystem
{
    public void Draw(GameTime gameTime, EntityManager world)
    {
        foreach (var e in world.Entities)
        {
            if (!e.TryGet(out SlimeComponent slime) || !e.TryGet(out SpriteComponent sprite))
                continue;

            for (int i = 0; i < slime.Segments.Count; i++)
            {
                var seg = slime.Segments[i];
                var pos = Vector2.Lerp(seg.At, seg.To, slime.MovementProgress);
                sprite.Sprite.Draw(Core.SpriteBatch, pos);
            }
        }
    }
}
