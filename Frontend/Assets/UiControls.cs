using UnityEngine;
using UnityEngine.UI;

public class UiControls : MonoBehaviour
{
    public Slider SliderMusicVolume;
    public Slider SliderFxVolume;

    void Start()
    {
        SliderMusicVolume.value = SoundManager.MusicVolume;
        SliderFxVolume.value = SoundManager.FxVolume;
    }

    public void FxVolumeChanged()
    {
        SoundManager.FxVolume = SliderFxVolume.value;
        SoundManager.Play(FxList.Instance.ClockTick);
    }

    public void MusicVolumeChanged()
        => SoundManager.MusicVolume = SliderMusicVolume.value;

    void SaveSettings()
    {
        SoundManager.SaveSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
    }
}
