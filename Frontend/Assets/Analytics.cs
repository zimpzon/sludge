using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Analytics : MonoBehaviour
{
    public static Analytics Instance;

    const float FlushIntervalSeconds = 60 * 5; // Max 100 events per hour - let's try flushing every 5 min.

    struct AggregatedEvent
    {
        public int count;
        public double sumTime;
    }

    Dictionary<string, AggregatedEvent> levelFail = new Dictionary<string, AggregatedEvent>();
    Dictionary<string, AggregatedEvent> levelComplete = new Dictionary<string, AggregatedEvent>();
    Dictionary<string, AggregatedEvent> replayStarted = new Dictionary<string, AggregatedEvent>();

    Dictionary<string, object> tempDictionary = new Dictionary<string, object>();

    double levelStartTime;
    float nextFlushTime;

    private void Awake()
    {
        Instance = this;
    }

    void CheckFlush()
    {
        bool isTimeToFlush = Time.time > nextFlushTime;
        if (isTimeToFlush)
        {
            foreach(var aggr in levelFail)
            {
                string level = aggr.Key;
                tempDictionary.Clear();
                tempDictionary["count"] = aggr.Value.count;
                tempDictionary["avg_time"] = (int)(aggr.Value.sumTime / aggr.Value.count);
                AnalyticsEvent.LevelFail(level, tempDictionary);
            }

            foreach (var aggr in levelComplete)
            {
                string level = aggr.Key;
                tempDictionary.Clear();
                tempDictionary["count"] = aggr.Value.count;
                tempDictionary["avg_time"] = (int)(aggr.Value.sumTime / aggr.Value.count);
                AnalyticsEvent.LevelComplete(level, tempDictionary);
            }

            foreach (var aggr in replayStarted)
            {
                string level = aggr.Key;
                tempDictionary.Clear();
                tempDictionary["count"] = aggr.Value.count;
                AnalyticsEvent.Custom("replay_level", tempDictionary);
            }

            nextFlushTime = Time.time + FlushIntervalSeconds;
        }
    }

    public void LevelStart()
    {
        levelStartTime = GameManager.Instance.EngineTime;
    }

    public void LevelFail(string levelName)
    {
        double time = GameManager.Instance.EngineTime - levelStartTime;

        levelFail.TryGetValue(levelName, out var aggregated);
        aggregated.count++;
        aggregated.sumTime += time;
        levelFail[levelName] = aggregated;

        CheckFlush();
    }

    public void LevelComplete(string levelName)
    {
        double time = GameManager.Instance.EngineTime - levelStartTime;

        levelComplete.TryGetValue(levelName, out var aggregated);
        aggregated.count++;
        aggregated.sumTime += time;
        levelComplete[levelName] = aggregated;

        CheckFlush();
    }

    public void ReplayStarted(string levelName)
    {
        double time = GameManager.Instance.EngineTime - levelStartTime;

        replayStarted.TryGetValue(levelName, out var aggregated);
        aggregated.count++;
        replayStarted[levelName] = aggregated;

        CheckFlush();
    }
}
