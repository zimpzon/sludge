using MoreMountains.Feedbacks;
using UnityEngine;

public class FeelTools : MonoBehaviour
{
    public static FeelTools I;

    MMF_Player _player;
    MMF_FloatingText _floatingText;

    private void Awake()
    {
        I = this;
        _player = GetComponent<MMF_Player>();
        _floatingText = _player.GetFeedbackOfType<MMF_FloatingText>();
    }

    public static void SpawnFloatingText(string text, Vector3 position, Color color)
    {
        I._floatingText.Value = text;
        I._player.PlayFeedbacks(position);
    }
}
