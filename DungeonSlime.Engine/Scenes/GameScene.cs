using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using DungeonSlime.Library;
using DungeonSlime.Library.Graphics;
using DungeonSlime.Library.Scenes;
using DungeonSlime.Engine.UI;
using DungeonSlime.Engine.Input;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.ECS.Systems;
using DungeonSlime.Engine.Models;

namespace DungeonSlime.Engine.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    // ECS world and systems
    private EntityManager _world;
    private SlimeSystem _slimeSystem;
    private BatSystem _batSystem;

    // ECS entities/components
    private Entity _slimeEntity;
    private TransformComponent _slimeTransform;
    private SpriteComponent _slimeSprite;
    private SlimeComponent _slime;

    private Entity _batEntity;
    private TransformComponent _batTransform;
    private SpriteComponent _batSprite;
    private BatComponent _bat;

    // Defines the tilemap to draw.
    private Tilemap _tilemap;

    // Defines the bounds of the room that the slime and bat are contained within.
    private Rectangle _roomBounds;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect;

    // Tracks the players score.
    private int _score;

    private GameSceneUI _ui;

    private GameState _state;

    // The grayscale shader effect.
    private Effect _grayscaleEffect;

    // The amount of saturation to provide the grayscale shader effect.
    private float _saturation = 1.0f;

    // The speed of the fade to grayscale effect.
    private const float FADE_SPEED = 0.02f;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // During the game scene, we want to disable exit on escape. Instead,
        // the escape key will be used to return back to the title screen.
        Core.ExitOnEscape = false;

        // Create the room bounds by getting the bounds of the screen then
        // using the Inflate method to "Deflate" the bounds by the width and
        // height of a tile so that the bounds only covers the inside room of
        // the dungeon tilemap.
        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        // Create any UI elements from the root element created in previous
        // scenes.
        GumService.Default.Root.Children.Clear();

        // Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize ECS systems/world
        _world = new EntityManager();
        _slimeSystem = new SlimeSystem();
        _batSystem = new BatSystem();

        // Initialize a new game to be played.
        InitializeNewGame();
    }

    private void InitializeUI()
    {
        // Clear out any previous UI element incase we came here
        // from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _ui = new GameSceneUI();

        // Subscribe to the events from the game scene ui.
        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.RetryButtonClick += OnRetryButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        // Change the game state back to playing.
        _state = GameState.Playing;
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        // Player has chosen to retry, so initialize a new game.
        InitializeNewGame();
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        // Player has chosen to quit, so return back to the title scene.
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeNewGame()
    {
        // Reset world state by creating fresh entities
        _world = new EntityManager();

        // Calculate the position for the slime, which will be at the center
        // tile of the tile map.
        Vector2 slimePos = new Vector2();
        slimePos.X = _tilemap.Columns / 2 * _tilemap.TileWidth;
        slimePos.Y = _tilemap.Rows / 2 * _tilemap.TileHeight;

        // Create slime entity/components
        _slimeEntity = _world.Create();
        _slimeTransform = new TransformComponent { Position = slimePos, Direction = Vector2.UnitX };
        _slimeSprite = new SpriteComponent { Sprite = _slimeAnim };
        _slime = new SlimeComponent
        {
            Stride = _tilemap.TileWidth,
            NextDirection = Vector2.UnitX,
            MovementTimer = TimeSpan.Zero,
            MovementProgress = 0f
        };
        // Initialize head segment
        var head = new SlimeSegment
        {
            At = slimePos,
            To = slimePos + new Vector2(_slime.Stride, 0),
            Direction = Vector2.UnitX
        };
        _slime.Segments.Add(head);
        _slime.BodyCollision += OnSlimeBodyCollision;

        _slimeEntity.Add(_slimeTransform);
        _slimeEntity.Add(_slimeSprite);
        _slimeEntity.Add(_slime);

        // Create bat entity/components
        _batEntity = _world.Create();
        _batTransform = new TransformComponent { Position = Vector2.Zero };
        _batSprite = new SpriteComponent { Sprite = _batAnim };
        _bat = new BatComponent { MovementSpeed = 5.0f, BounceSoundEffect = _bounceSfx };

        _batEntity.Add(_batTransform);
        _batEntity.Add(_batSprite);
        _batEntity.Add(_bat);

        // Initialize the bat.
        BatSystem.RandomizeVelocity(_bat);
        PositionBatAwayFromSlime();

        // Reset the score.
        _score = 0;

        // Set the game state to playing.
        _state = GameState.Playing;
    }

    // Local cached assets created in LoadContent
    private AnimatedSprite _slimeAnim;
    private AnimatedSprite _batAnim;
    private SoundEffect _bounceSfx;

    public override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file.
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        // Create the tilemap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Create and cache the animated sprites (kept immutable originals)
        _slimeAnim = atlas.CreateAnimatedSprite("slime-animation");
        _slimeAnim.Scale = new Vector2(4.0f, 4.0f);

        _batAnim = atlas.CreateAnimatedSprite("bat-animation");
        _batAnim.Scale = new Vector2(4.0f, 4.0f);

        // Load the bounce sound effect for the bat.
        _bounceSfx = Content.Load<SoundEffect>("audio/bounce");

        // Load the collect sound effect.
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        // Load the grayscale effect.
        _grayscaleEffect = Content.Load<Effect>("effects/grayscaleEffect");
    }

    public override void Update(GameTime gameTime)
    {
        // Ensure the UI is always updated.
        _ui.Update(gameTime);

        if (_state != GameState.Playing)
        {
            // The game is in either a paused or game over state, so
            // gradually decrease the saturation to create the fading grayscale.
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);

            // If its just a game over state, return back.
            if (_state == GameState.GameOver)
            {
                return;
            }
        }

        // If the pause button is pressed, toggle the pause state.
        if (GameController.Pause())
        {
            TogglePause();
        }

        // At this point, if the game is paused, just return back early.
        if (_state == GameState.Paused)
        {
            return;
        }

        // Update systems
        _slimeSystem.Update(gameTime, _world);
        _batSystem.Update(gameTime, _world, _roomBounds);

        // Perform collision checks.
        CollisionChecks();
    }

    private void CollisionChecks()
    {
        // Capture the current bounds of the slime and bat.
        Circle slimeBounds = GetSlimeBounds();
        Circle batBounds = GetBatBounds();

        // First perform a collision check to see if the slime is colliding with the bat
        if (slimeBounds.Intersects(batBounds))
        {
            // Move the bat to a new position away from the slime.
            PositionBatAwayFromSlime();

            // Randomize the velocity of the bat.
            BatSystem.RandomizeVelocity(_bat);

            // Tell the slime to grow by adding a segment at the tail
            GrowSlime();

            // Increment the score.
            _score += 100;

            // Update the score display on the UI.
            _ui.UpdateScoreText(_score);

            // Play the collect sound effect.
            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }

        // Next check if the slime is colliding with the wall by validating if
        // it is within the bounds of the room. If outside, trigger game over.
        if (slimeBounds.Top < _roomBounds.Top ||
            slimeBounds.Bottom > _roomBounds.Bottom ||
            slimeBounds.Left < _roomBounds.Left ||
            slimeBounds.Right > _roomBounds.Right)
        {
            GameOver();
            return;
        }
    }

    private void GrowSlime()
    {
        // Capture the tail
        var tail = _slime.Segments[^1];
        var basePos = tail.To + tail.ReverseDirection * _slime.Stride;
        var newTail = new SlimeSegment
        {
            At = basePos,
            To = tail.At,
            Direction = Vector2.Normalize(tail.At - basePos)
        };
        _slime.Segments.Add(newTail);
    }

    private Circle GetSlimeBounds()
    {
        var head = _slime.Segments[0];
        Vector2 pos = Vector2.Lerp(head.At, head.To, _slime.MovementProgress);
        return new Circle(
            (int)(pos.X + _slimeSprite.Sprite.Width * 0.5f),
            (int)(pos.Y + _slimeSprite.Sprite.Height * 0.5f),
            (int)(_slimeSprite.Sprite.Width * 0.5f)
        );
    }

    private Circle GetBatBounds()
    {
        int x = (int)(_batTransform.Position.X + _batSprite.Sprite.Width * 0.5f);
        int y = (int)(_batTransform.Position.Y + _batSprite.Sprite.Height * 0.5f);
        int radius = (int)(_batSprite.Sprite.Width * 0.25f);
        return new Circle(x, y, radius);
    }

    private void PositionBatAwayFromSlime()
    {
        // Calculate the position that is in the center of the bounds
        float roomCenterX = _roomBounds.X + _roomBounds.Width * 0.5f;
        float roomCenterY = _roomBounds.Y + _roomBounds.Height * 0.5f;
        Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);

        // Get slime center
        Circle slimeBounds = GetSlimeBounds();
        Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);

        // Vector from room center to slime
        Vector2 centerToSlime = slimeCenter - roomCenter;

        // Bat bounds and padding
        Circle batBounds = GetBatBounds();
        int padding = batBounds.Radius * 2;

        Vector2 newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            newBatPosition.Y = Random.Shared.Next(
                _roomBounds.Top + padding,
                _roomBounds.Bottom - padding
            );

            if (centerToSlime.X > 0)
            {
                newBatPosition.X = _roomBounds.Left + padding;
            }
            else
            {
                newBatPosition.X = _roomBounds.Right - padding * 2;
            }
        }
        else
        {
            newBatPosition.X = Random.Shared.Next(
                _roomBounds.Left + padding,
                _roomBounds.Right - padding
            );

            if (centerToSlime.Y > 0)
            {
                newBatPosition.Y = _roomBounds.Top + padding;
            }
            else
            {
                newBatPosition.Y = _roomBounds.Bottom - padding * 2;
            }
        }

        _batTransform.Position = newBatPosition;
    }

    private void OnSlimeBodyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            // We're now unpausing the game, so hide the pause panel.
            _ui.HidePausePanel();

            // And set the state back to playing.
            _state = GameState.Playing;
        }
        else
        {
            // We're now pausing the game, so show the pause panel.
            _ui.ShowPausePanel();

            // And set the state to paused.
            _state = GameState.Paused;

            // Set the grayscale effect saturation to 1.0f
            _saturation = 1.0f;
        }
    }

    private void GameOver()
    {
        // Show the game over panel.
        _ui.ShowGameOverPanel();

        // Set the game state to game over.
        _state = GameState.GameOver;

        // Set the grayscale effect saturation to 1.0f
        _saturation = 1.0f;
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_state != GameState.Playing)
        {
            // We are in a game over state, so apply the saturation parameter.
            _grayscaleEffect.Parameters["Saturation"].SetValue(_saturation);

            // And begin the sprite batch using the grayscale effect.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _grayscaleEffect);
        }
        else
        {
            // Otherwise, just begin the sprite batch as normal.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);

        // Draw the slime segments
        foreach (var segment in _slime.Segments)
        {
            Vector2 pos = Vector2.Lerp(segment.At, segment.To, _slime.MovementProgress);
            _slimeSprite.Sprite.Draw(Core.SpriteBatch, pos);
        }

        // Draw the bat
        _batSprite.Sprite.Draw(Core.SpriteBatch, _batTransform.Position);

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        // Draw the UI.
        _ui.Draw();
    }
}
