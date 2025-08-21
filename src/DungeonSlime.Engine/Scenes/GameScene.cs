using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using DungeonSlime.Library;
using DungeonSlime.Library.Graphics;
using DungeonSlime.Engine.UI;
using DungeonSlime.Engine.Input;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.ECS.Systems;
using DungeonSlime.Engine.Models;
using DungeonSlime.Library.Geometry;

namespace DungeonSlime.Engine.Scenes;

public class GameScene : EcsSceneBase
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    // Inicializa los campos de sistema ECS como nullables para evitar la advertencia CS8618.
    // Se garantiza su inicialización en Initialize().
    private SlimeSystem? _slimeSystem;
    private BatSystem? _batSystem;
    private CollectSystem? _collectSystem;
    private RenderSystem? _renderSystem;
    private SlimeRenderSystem? _slimeRenderSystem;
    private SlimeBoundsSystem? _slimeBoundsSystem; // new system to decouple bounds checking

    // Plan (pseudocode):
    // - Suppress CS8618 for fields assigned in InitializeNewGame/LoadContent.
    // - Use null-forgiving operator (= null!) to indicate late initialization.
    // - Do not change runtime behavior.

    private Entity _slimeEntity = null!;
    private TransformComponent _slimeTransform = null!;
    private SpriteComponent _slimeSprite = null!;
    private SlimeComponent _slime = null!;

    private Entity _batEntity = null!;
    private TransformComponent _batTransform = null!;
    private SpriteComponent _batSprite = null!;
    private BatComponent _bat = null!;

    private Entity _roomEntity = null!;
    private Entity _scoreEntity = null!;
    private Entity _gameStateEntity = null!;
    private ScoreComponent _scoreComponent = null!; // cache to avoid per-frame world iteration
    private GameStateComponent _gameStateComponent = null!;

    // Defines the tilemap to draw.
    private Tilemap _tilemap = null!;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect = null!;

    // Tracks the players score.
    private int _score;

    private GameSceneUI? _ui;

    private GameState _state;

    // The grayscale shader effect.
    private Effect? _grayscaleEffect;
    private EffectParameter? _grayscaleSaturationParam; // cached parameter lookup

    // The amount of saturation to provide the grayscale shader effect.
    private float _saturation = 1.0f;

    // The speed of the fade to grayscale effect.
    private const float FADE_SPEED = 0.02f;

    private Rectangle _roomBounds;

    // Local cached assets created in LoadContent
    private AnimatedSprite? _slimeAnim;
    private AnimatedSprite? _batAnim;
    private SoundEffect? _bounceSfx;
    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        Core.ExitOnEscape = false;

        InitializeUI();

        _slimeSystem = new SlimeSystem();
        _batSystem = new BatSystem();
        _collectSystem = new CollectSystem();
        _renderSystem = new RenderSystem();
        _slimeRenderSystem = new SlimeRenderSystem();
        _slimeBoundsSystem = new SlimeBoundsSystem();

        InitializeNewGame();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();
        _ui = new GameSceneUI();
        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.RetryButtonClick += OnRetryButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    private void OnResumeButtonClicked(object? sender, EventArgs args)
    {
        _state = GameState.Playing;
    }

    private void OnRetryButtonClicked(object? sender, EventArgs args)
    {
        InitializeNewGame();
    }

    private void OnQuitButtonClicked(object? sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
    }

    private void RegisterAllSystems()
    {
        if (_slimeSystem is not null) RegisterSystem(_slimeSystem);
        if (_batSystem is not null) RegisterSystem(_batSystem);
        if (_collectSystem is not null) RegisterSystem(_collectSystem);
        if (_slimeBoundsSystem is not null) RegisterSystem(_slimeBoundsSystem);
    }

    private void RegisterAllRenderSystems()
    {
        if (_renderSystem is not null) RegisterRenderSystem(_renderSystem);
        if (_slimeRenderSystem is not null) RegisterRenderSystem(_slimeRenderSystem);
    }

    private void InitializeNewGame()
    {
        // Reset ECS world and re-register systems to ensure a clean state
        ResetWorld();
        RegisterAllSystems();
        RegisterAllRenderSystems();

        // Create room bounds entity (deflated to inside of dungeon)
        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);
        _roomEntity = World.Create();
        _roomEntity.Add(new RoomBoundsComponent { Bounds = _roomBounds });

        // Create score holder entity and cache its component
        _scoreEntity = World.Create();
        _scoreComponent = new ScoreComponent { Score = 0 };
        _scoreEntity.Add(_scoreComponent);

        // Create shared game state entity
        _gameStateEntity = World.Create();
        _gameStateComponent = new GameStateComponent();
        _gameStateEntity.Add(_gameStateComponent);

        // Center tile position for slime
        Vector2 slimePos = new Vector2(
            _tilemap.Columns / 2 * _tilemap.TileWidth,
            _tilemap.Rows / 2 * _tilemap.TileHeight
        );

        // Slime entity
        _slimeEntity = World.Create();
        _slimeTransform = new TransformComponent { Position = slimePos, Direction = Vector2.UnitX };
        if (_slimeAnim is null || _batAnim is null)
            throw new InvalidOperationException("Animated sprites must be loaded before initializing a new game.");

        // Ensure the slime sprite component is created
        _slimeSprite = new SpriteComponent { Sprite = _slimeAnim };

        _slime = new SlimeComponent
        {
            Stride = _tilemap.TileWidth,
            NextDirection = Vector2.UnitX,
            MovementTimer = TimeSpan.Zero,
            MovementProgress = 0f
        };
        var head = new SlimeSegment { At = slimePos, To = slimePos + new Vector2(_slime.Stride, 0), Direction = Vector2.UnitX };
        _slime.Segments.Add(head);
        _slimeEntity.Add(_slimeTransform);
        _slimeEntity.Add(_slimeSprite);
        _slimeEntity.Add(_slime);

        // Bat entity
        _batEntity = World.Create();
        _batTransform = new TransformComponent { Position = Vector2.Zero };
        _batSprite = new SpriteComponent { Sprite = _batAnim };
        _bat = new BatComponent { MovementSpeed = 5.0f, BounceSoundEffect = _bounceSfx };
        _batEntity.Add(_batTransform);
        _batEntity.Add(_batSprite);
        _batEntity.Add(_bat);

        BatSystem.RandomizeVelocity(_bat);
        PositionBatAwayFromSlime();

        _score = 0;
        _state = GameState.Playing;
    }

    public override void LoadContent()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        _slimeAnim = atlas.CreateAnimatedSprite("slime-animation");
        _slimeAnim.Scale = new Vector2(4.0f, 4.0f);

        _batAnim = atlas.CreateAnimatedSprite("bat-animation");
        _batAnim.Scale = new Vector2(4.0f, 4.0f);

        _bounceSfx = Content.Load<SoundEffect>("audio/bounce");
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");
        _grayscaleEffect = Content.Load<Effect>("effects/grayscaleEffect");
        _grayscaleSaturationParam = _grayscaleEffect?.Parameters["Saturation"]; // cache param for fast access
    }

    public override void Update(GameTime gameTime)
    {
        // Update UI always
        _ui?.Update(gameTime);

        if (_state != GameState.Playing)
        {
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);
            if (_state == GameState.GameOver)
                return;
        }

        if (GameController.Pause())
        {
            TogglePause();
        }

        if (_state == GameState.Paused)
        {
            return;
        }

        // Run base ECS loop (updates registered systems)
        base.Update(gameTime);

        // If slime collided with its body, trigger game over and reset flag
        if (_slime.BodyCollisionDetected)
        {
            _slime.BodyCollisionDetected = false;
            GameOver();
            return;
        }

        // If a system flagged game over (e.g., bounds system), handle it once
        if (_gameStateComponent.IsGameOver)
        {
            _gameStateComponent.IsGameOver = false;
            GameOver();
            return;
        }

        // Sync score stored in ECS to UI value
        SyncScoreToUi();

        // Room bounds are managed by component; refresh local cache if scene resized
        _roomBounds = _roomEntity.Get<RoomBoundsComponent>().Bounds;
    }

    private void SyncScoreToUi()
    {
        // Use cached component instead of iterating all entities
        if (_scoreComponent is null)
            return;

        if (_score != _scoreComponent.Score)
        {
            _score = _scoreComponent.Score;
            _ui?.UpdateScoreText(_score);
            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }
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

    private void PositionBatAwayFromSlime()
    {
        var slimeBounds = GetSlimeBounds();
        var r = _roomBounds;
        float roomCenterX = r.X + r.Width * 0.5f;
        float roomCenterY = r.Y + r.Height * 0.5f;
        Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);
        Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);
        Vector2 centerToSlime = slimeCenter - roomCenter;
        var batBoundsRadius = (int)(_batSprite.Sprite.Width * 0.25f);
        int padding = batBoundsRadius * 2;

        Vector2 newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            newBatPosition.Y = Random.Shared.Next(r.Top + padding, r.Bottom - padding);
            newBatPosition.X = centerToSlime.X > 0 ? r.Left + padding : r.Right - padding * 2;
        }
        else
        {
            newBatPosition.X = Random.Shared.Next(r.Left + padding, r.Right - padding);
            newBatPosition.Y = centerToSlime.Y > 0 ? r.Top + padding : r.Bottom - padding * 2;
        }

        _batTransform.Position = newBatPosition;
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            _ui?.HidePausePanel();
            _state = GameState.Playing;
        }
        else
        {
            _ui?.ShowPausePanel();
            _state = GameState.Paused;
            _saturation = 1.0f;
        }
    }

    private void GameOver()
    {
        _ui?.ShowGameOverPanel();
        _state = GameState.GameOver;
        _saturation = 1.0f;
    }

    private void BeginSpriteBatchForState()
    {
        if (_state != GameState.Playing)
        {
            if (_grayscaleEffect != null)
            {
                _grayscaleSaturationParam?.SetValue(_saturation);
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _grayscaleEffect);
            }
            else
            {
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            }
        }
        else
        {
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        BeginSpriteBatchForState();
        _tilemap.Draw(Core.SpriteBatch);

        // Draw ECS entities (RenderSystem + SlimeRenderSystem)
        base.Draw(gameTime);

        Core.SpriteBatch.End();

        // Draw UI
        _ui?.Draw();
    }
}
