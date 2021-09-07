using UnityEngine;
using UnityEngine.UI;

public class UiControls : MonoBehaviour
{
    public Slider SliderMusicVolume;
    public Slider SliderFxVolume;
    bool isInit = true;

    void Start()
    {
        SliderMusicVolume.value = SoundManager.MusicVolume;
        SliderFxVolume.value = SoundManager.FxVolume;
        isInit = false;
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

    private void OnDisable()
    {
        SaveSettings();
    }
}
