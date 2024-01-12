using MoreMountains.Feedbacks;
using UnityEngine;

public class FeelTools : MonoBehaviour
{
    // channel 1: game event floating text
    // channel 2: mutator floating text

    public static FeelTools I;

    public MMF_Player GameEventFloatingTextPlayer;
    public MMF_Player MutatorFloatingTextPlayer;

    MMF_FloatingText _gameEventFloatingText;
    MMF_FloatingText _mutatorFloatingText;

    private void Awake()
    {
        I = this;

        _gameEventFloatingText = GameEventFloatingTextPlayer.GetFeedbackOfType<MMF_FloatingText>();
        _mutatorFloatingText = MutatorFloatingTextPlayer.GetFeedbackOfType<MMF_FloatingText>();
    }

    public static void SpawnGameEventFloatingText(string text, Vector3 position)
        => FloatingText(text, position, I.GameEventFloatingTextPlayer, I._gameEventFloatingText);

    public static void SpawnMutatorFloatingText(string text, Vector3 position)
        => FloatingText(text, position, I.MutatorFloatingTextPlayer, I._mutatorFloatingText);

    private static void FloatingText(string text, Vector3 position, MMF_Player player, MMF_FloatingText floatingText)
    {
        floatingText.Value = text;
        player.PlayFeedbacks(position);
    }
}
