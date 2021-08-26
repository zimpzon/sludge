using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Sludge.Utility
{
    public static class Startup
    {
        public static void StaticInit()
        {
            Strings.Init();
            LevelList.LoadLevels();
            PlayerProgress.Load();
            DOTween.Init();
            Physics2D.simulationMode = SimulationMode2D.Script;
            GameManager.ClientId = PlayerPrefs.GetString("ClientId", Guid.NewGuid().ToString());

            AnalyticsEvent.Custom("client_start",
                new Dictionary<string, object> {
                    { "client_id", GameManager.ClientId }
                });
        }
    }
}
