using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Systems;

public interface IRenderSystem
{
    void Draw(GameTime gameTime, EntityManager world);
}
