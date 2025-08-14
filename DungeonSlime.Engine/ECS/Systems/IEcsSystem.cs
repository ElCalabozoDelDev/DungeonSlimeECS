using Microsoft.Xna.Framework;

namespace DungeonSlime.Engine.ECS.Systems;

public interface IEcsSystem
{
    void Update(GameTime gameTime, EntityManager world);
}
