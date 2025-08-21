using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Systems;

public interface IUpdateSystem
{
    void Update(GameTime gameTime, EntityManager world);
}
