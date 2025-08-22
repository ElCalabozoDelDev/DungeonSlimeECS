using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.UI;

namespace DungeonSlime.Engine.Scenes;

/// <summary>
/// Manages game state transitions and related visual effects.
/// </summary>
public class GameStateManager
{
    private readonly GameSceneUI _ui;
    private readonly GameStateComponent _gameState;
    
    private float _saturation = 1.0f;
    private const float FADE_SPEED = 0.02f;

    public GameStateManager(GameSceneUI ui, GameStateComponent gameState)
    {
        _ui = ui;
        _gameState = gameState;
    }

    public void TogglePause()
    {
        if (_gameState.State == GameState.Paused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        _ui.ShowPausePanel();
        _gameState.State = GameState.Paused;
        _saturation = 1.0f;
    }

    public void Resume()
    {
        _ui.HidePausePanel();
        _gameState.State = GameState.Playing;
    }

    public void GameOver()
    {
        _ui.ShowGameOverPanel();
        _gameState.State = GameState.GameOver;
        _saturation = 1.0f;
    }

    public void UpdateVisualEffects(GameTime gameTime)
    {
        if (_gameState.State != GameState.Playing)
        {
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);
        }
    }

    public float CurrentSaturation => _saturation;
    public GameState CurrentState => _gameState.State;
    public bool ShouldUpdateGameplay => _gameState.State == GameState.Playing;
    public bool ShouldExitEarly => _gameState.State == GameState.GameOver;
}