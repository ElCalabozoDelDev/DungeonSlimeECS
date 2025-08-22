using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DungeonSlime.Library.Graphics;

namespace DungeonSlime.Engine.Scenes;

/// <summary>
/// Manages loading and organizing game content for the game scene.
/// </summary>
public class GameContentManager
{
    public Tilemap Tilemap { get; private set; } = null!;
    public AnimatedSprite SlimeAnimation { get; private set; } = null!;
    public AnimatedSprite BatAnimation { get; private set; } = null!;
    public SoundEffect BounceSfx { get; private set; } = null!;
    public SoundEffect CollectSfx { get; private set; } = null!;
    public Effect? GrayscaleEffect { get; private set; }
    public EffectParameter? GrayscaleSaturationParam { get; private set; }

    public void LoadContent(ContentManager content)
    {
        var atlas = TextureAtlas.FromFile(content, "images/atlas-definition.xml");

        // Load tilemap
        Tilemap = Tilemap.FromFile(content, "images/tilemap-definition.xml");
        Tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Load animations
        SlimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        SlimeAnimation.Scale = new Vector2(4.0f, 4.0f);

        BatAnimation = atlas.CreateAnimatedSprite("bat-animation");
        BatAnimation.Scale = new Vector2(4.0f, 4.0f);

        // Load audio
        BounceSfx = content.Load<SoundEffect>("audio/bounce");
        CollectSfx = content.Load<SoundEffect>("audio/collect");

        // Load effects
        GrayscaleEffect = content.Load<Effect>("effects/grayscaleEffect");
        GrayscaleSaturationParam = GrayscaleEffect?.Parameters["Saturation"];
    }

    public Vector2 GetSlimeStartPosition()
    {
        return new Vector2(
            Tilemap.Columns / 2 * Tilemap.TileWidth,
            Tilemap.Rows / 2 * Tilemap.TileHeight
        );
    }

    public Rectangle GetRoomBounds(Rectangle screenBounds)
    {
        var bounds = screenBounds;
        bounds.Inflate(-Tilemap.TileWidth, -Tilemap.TileHeight);
        return bounds;
    }
}