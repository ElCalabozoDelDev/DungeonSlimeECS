using DungeonSlime.Engine.ECS;
using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.Contracts;

public interface IUpdateSystem
{
    void Update(GameTime gameTime, EntityManager world);
}
