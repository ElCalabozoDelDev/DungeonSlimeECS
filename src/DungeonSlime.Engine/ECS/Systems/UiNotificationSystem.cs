using Microsoft.Xna.Framework;
using DungeonSlime.Engine.ECS.Components;
using DungeonSlime.Engine.UI;
using DungeonSlime.Library;

namespace DungeonSlime.Engine.ECS.Systems;

/// <summary>
/// Observes score changes and game over flags to update UI via UiEvents and play SFX via GameAudioComponent.
/// Keeps scene decoupled from ECS logic.
/// </summary>
public class UiNotificationSystem : IUpdateSystem
{
    private int _lastScore;

    public void Update(GameTime gameTime, EntityManager world)
    {
        ScoreComponent? score = null;
        GameStateComponent? state = null;
        GameAudioComponent? audio = null;

        foreach (var e in world.Entities)
        {
            if (score == null && e.TryGet(out ScoreComponent sc)) score = sc;
            if (state == null && e.TryGet(out GameStateComponent gs)) state = gs;
            if (audio == null && e.TryGet(out GameAudioComponent ga)) audio = ga;
            if (score != null && state != null && audio != null) break;
        }

        if (score != null && score.Score != _lastScore)
        {
            _lastScore = score.Score;
            UiEvents.RaiseScoreChanged(_lastScore);
            if (audio?.CollectSoundEffect != null)
            {
                Core.Audio.PlaySoundEffect(audio.CollectSoundEffect);
            }
        }

        if (state != null && state.IsGameOver)
        {
            state.IsGameOver = false; // consume the flag so it only fires once
            UiEvents.RaiseGameOver();
        }
    }
}
