using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Analytics;

namespace Sludge.Utility
{
    public static class Startup
    {
        public static void StaticInit()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Strings.Init();
            LevelList.LoadLevels();
            PlayerProgress.Load();
            DOTween.Init();
            Physics2D.simulationMode = SimulationMode2D.Script;
            GameManager.ClientId = PlayerPrefs.GetString("ClientId", Guid.NewGuid().ToString());
            PlayerPrefs.Save();

            QualitySettings.vSyncCount = 1;

            AnalyticsEvent.Custom("client_start",
                new Dictionary<string, object> {
                    { "client_id", GameManager.ClientId }
                });
        }
    }
}
