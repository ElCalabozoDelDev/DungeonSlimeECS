using Microsoft.Xna.Framework.Audio;

namespace DungeonSlime.Engine.ECS.Components;

/// <summary>
/// Provides references to globally used sound effects for systems to play.
/// </summary>
public class GameAudioComponent
{
    public SoundEffect? CollectSoundEffect { get; set; }
}
