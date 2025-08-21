using DungeonSlime.Engine.ECS;
using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.Contracts;

public interface IRenderSystem
{
    void Draw(GameTime gameTime, EntityManager world);
}
