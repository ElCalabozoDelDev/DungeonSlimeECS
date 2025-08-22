using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using DungeonSlime.Library;
using DungeonSlime.Engine.UI;
using DungeonSlime.Engine.Input;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.ECS.Systems;

namespace DungeonSlime.Engine.Scenes;

public class GameScene : EcsSceneBase
{
    // Core managers
    private GameContentManager _contentManager = null!;
    private GameSystemRegistry _systemRegistry = null!;
    private GameStateManager? _stateManager;
    private GameSceneUI? _ui;

    // Cached entity references for direct access
    private Entity _slimeEntity = null!;
    private Entity _batEntity = null!;
    private Entity _gameStateEntity = null!;
    private GameStateComponent _gameStateComponent = null!;
    private ScoreComponent _scoreComponent = null!;

    public override void Initialize()
    {
        // Initialize managers BEFORE calling base.Initialize() which calls LoadContent
        _contentManager = new GameContentManager();
        _systemRegistry = new GameSystemRegistry();
        
        // LoadContent is called during base.Initialize().
        base.Initialize();
        
        Core.ExitOnEscape = false;
        
        InitializeUI();
        InitializeNewGame();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();
        _ui = new GameSceneUI();
        _ui.ResumeButtonClick += (_, _) => _stateManager?.Resume();
        _ui.RetryButtonClick += (_, _) => InitializeNewGame();
        _ui.QuitButtonClick += (_, _) => Core.ChangeScene(new TitleScene());

        // Subscribe to UI events from systems
        UiEvents.ScoreChanged += score => _ui?.UpdateScoreText(score);
        UiEvents.GameOver += () => _stateManager?.GameOver();
    }

    private void InitializeNewGame()
    {
        // Reset ECS world and systems
        ResetWorld();
        _systemRegistry.Clear();
        _systemRegistry.RegisterGameplaySystems();
        _systemRegistry.RegisterRenderSystems();
        
        // Register systems using base class methods
        foreach (var system in _systemRegistry.UpdateSystems)
        {
            RegisterSystem(system);
        }
        foreach (var system in _systemRegistry.RenderSystems)
        {
            RegisterRenderSystem(system);
        }

        // Create game entities using factory
        var roomBounds = _contentManager.GetRoomBounds(Core.GraphicsDevice.PresentationParameters.Bounds);
        GameEntityFactory.CreateRoomBounds(World, roomBounds);

        var scoreEntity = GameEntityFactory.CreateScore(World);
        _scoreComponent = scoreEntity.Get<ScoreComponent>();

        _gameStateEntity = GameEntityFactory.CreateGameState(World);
        _gameStateComponent = _gameStateEntity.Get<GameStateComponent>();

        GameEntityFactory.CreateGameAudio(World, _contentManager.CollectSfx);

        // Create game entities
        var slimePos = _contentManager.GetSlimeStartPosition();
        _slimeEntity = GameEntityFactory.CreateSlime(World, slimePos, _contentManager.Tilemap.TileWidth, _contentManager.SlimeAnimation);

        _batEntity = GameEntityFactory.CreateBat(World, _contentManager.BatAnimation, _contentManager.BounceSfx);
        
        // Get BatSystem from registry and randomize velocity
        var batSystem = _systemRegistry.GetSystem<BatSystem>();
        if (batSystem != null)
        {
            BatSystem.RandomizeVelocity(_batEntity.Get<BatComponent>());
        }

        // Initialize state manager
        if (_ui != null)
        {
            _stateManager = new GameStateManager(_ui, _gameStateComponent);
        }

        _gameStateComponent.State = GameState.Playing;
    }

    public override void LoadContent()
    {
        _contentManager.LoadContent(Content);
    }

    public override void Update(GameTime gameTime)
    {
        _ui?.Update(gameTime);

        _stateManager?.UpdateVisualEffects(gameTime);

        if (_stateManager?.ShouldExitEarly == true)
            return;

        if (GameController.Pause())
        {
            _stateManager?.TogglePause();
        }

        if (_stateManager?.ShouldUpdateGameplay != true)
            return;

        // Run ECS systems
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        BeginSpriteBatchForState();
        _contentManager.Tilemap.Draw(Core.SpriteBatch);

        // Draw ECS entities
        base.Draw(gameTime);

        Core.SpriteBatch.End();

        // Draw UI
        _ui?.Draw();
    }

    private void BeginSpriteBatchForState()
    {
        if (_stateManager?.CurrentState != GameState.Playing && _contentManager.GrayscaleEffect != null)
        {
            _contentManager.GrayscaleSaturationParam?.SetValue(_stateManager.CurrentSaturation);
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _contentManager.GrayscaleEffect);
        }
        else
        {
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }
    }
}
