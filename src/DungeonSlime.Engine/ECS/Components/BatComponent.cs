using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DungeonSlime.Engine.ECS.Components;

public class BatComponent
{
    /// <summary>
    /// Current velocity of the bat in world units per update.
    /// </summary>
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// Movement speed scalar used when randomizing velocity.
    /// </summary>
    public float MovementSpeed { get; set; } = 5.0f;

    /// <summary>
    /// The sound effect to play when the bat bounces off a wall.
    /// </summary>
    public SoundEffect? BounceSoundEffect { get; set; }

    /// <summary>
    /// Indicates the bat needs an initial placement away from the slime.
    /// Systems should set this to false after placing it.
    /// </summary>
    public bool NeedsInitialPlacement { get; set; } = true;
}
