using System.Linq;
using UnityEngine;

public class FxList : MonoBehaviour
{
    public static FxList Instance;

    public SoundItem UiNope;
    public SoundItem UiShowMenu;
    public SoundItem UiHideMenu;
    public SoundItem UiChangeSelection;

    public SoundItem StartRound;
    public SoundItem PlayerDie;
    public SoundItem PlayerShoot;
    public SoundItem PlayerLanding;
    public SoundItem PlayerLanded;
    public SoundItem LevelComplete;
    public SoundItem SlimeBombExplode;
    public SoundItem EnemyShoot;
    public SoundItem EnemyDie;
    public SoundItem SnifferActivate;
    public SoundItem KeyPickup;
    public SoundItem PortalEnter;
    public SoundItem TimePillPickup;
    public SoundItem FakeWallDisappear;
    public SoundItem FakeWallShowUp;
    public SoundItem ThrownBombPickedUp;
    public SoundItem ThrownBombExplode;
    public SoundItem LaserHumming;
    public SoundItem ClockTick;
    public SoundItem Countdown5;
    public SoundItem Countdown4;
    public SoundItem Countdown3;
    public SoundItem Countdown2;
    public SoundItem Countdown1;
    public SoundItem BallCollectorSpawn;
    public SoundItem BallCollectorDie;

    private void Awake()
    {
        Instance = this;

        var sounds = typeof(FxList).GetFields().Where(p => p.FieldType == typeof(SoundItem)).ToList();
        foreach (var soundField in sounds)
        {
            var stuff = (SoundItem)soundField.GetValue(this);
            stuff.audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
}
