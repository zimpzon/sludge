using UnityEngine;

public class FxList : MonoBehaviour
{
    public static FxList Instance;

    public SoundItem UiNope;
    public SoundItem UiExecuteSelected;
    public SoundItem UiChangeSelection;
    public SoundItem StartRound;
    public SoundItem PlayerDie;
    public SoundItem LevelCompleteBad;
    public SoundItem LevelCompleteGood;
    public SoundItem SlimeBombExplode;
    public SoundItem EnemyShoot;
    public SoundItem EnemyDie;
    public SoundItem KeyPickup;
    public SoundItem PortalEnter;
    public SoundItem TimePillPickup;
    public SoundItem FakeWallDisappear;
    public SoundItem FakeWallShowUp;
    public SoundItem ThrownBombThrown;
    public SoundItem ThrownBombExplode;
    public SoundItem LaserHumming;
    public SoundItem Countdown5;
    public SoundItem Countdown4;
    public SoundItem Countdown3;
    public SoundItem Countdown2;
    public SoundItem Countdown1;

    private void Awake()
    {
        Instance = this;
    }
}
