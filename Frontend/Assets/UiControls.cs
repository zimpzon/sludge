using Assets.Scripts;
using Sludge.Utility;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiControls : MonoBehaviour
{
    public Slider SliderMusicVolume;
    public Slider SliderFxVolume;
    public TMP_Text TextStats;

    bool isInit = true;

    void Start()
    {
        SliderMusicVolume.value = SoundManager.MusicVolume;
        SliderFxVolume.value = SoundManager.FxVolume;
        isInit = false;
    }

    private void OnEnable()
    {
        FillStats();
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    public void FxVolumeChanged()
    {
        if (isInit)
            return;

        SoundManager.FxVolume = SliderFxVolume.value;
        SoundManager.Play(FxList.Instance.ClockTick);
    }

    public void MusicVolumeChanged()
    {
        if (isInit)
            return;
        SoundManager.MusicVolume = SliderMusicVolume.value;
    }

    void SaveSettings()
    {
        SoundManager.SaveSettings();
    }

    void FillStats()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Attempts\t{PlayerProgress.saveGame.TotalAttempts,7}");
        sb.AppendLine($"Deaths\t{PlayerProgress.saveGame.TotalDeaths,7}");
        sb.AppendLine();

        foreach (var deathTypeName in PlayerDeathTypeExtensions.Names)
        {
            if (deathTypeName.Key == PlayerDeathType.None)
                continue;

            if (PlayerProgress.saveGame.DeathsByType.TryGetValue(deathTypeName.Key, out int count) && count > 0)
            {
                sb.AppendLine($"{deathTypeName.Value}\t{count,7}");
            }
            else
            {
                sb.AppendLine($"{deathTypeName.Value}\t{0,7}");
            }
        }
        TextStats.text = sb.ToString();
    }
}
