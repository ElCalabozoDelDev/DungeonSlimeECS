using System;

namespace DungeonSlime.Engine.UI;

/// <summary>
/// Static UI event aggregator to decouple ECS systems from scene/UI implementation.
/// </summary>
public static class UiEvents
{
    public static event Action<int>? ScoreChanged;
    public static event Action? GameOver;

    public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    public static void RaiseGameOver() => GameOver?.Invoke();
}
