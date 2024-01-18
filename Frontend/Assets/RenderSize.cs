using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderSize : MonoBehaviour
{
    public Camera Camera;

    UniversalRenderPipelineAsset urp;

    private void Awake()
    {
        Camera = Camera.main;
        urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1;
        Debug.Log("TODO: lower render scale on 4K etc?");
    }

    bool showStats;
    double exponentialAvg;

    private void Update()
    {
        StatsDisplay();
    }

    void StatsDisplay()
    {
        if (Input.GetKeyDown(KeyCode.I) && Input.GetKey(KeyCode.LeftControl))
        {
            showStats = !showStats;
            if (!showStats)
                DebugLinesScript.Instance.Clear();
        }

        if (showStats)
        {
            DebugLinesScript.Show("fps", (int)(1 / Time.unscaledDeltaTime));
            DebugLinesScript.Show("avg", exponentialAvg);
            DebugLinesScript.Show("screen.currentResolution", Screen.currentResolution);
            DebugLinesScript.Show("screen.w/h", new Vector2(Screen.width, Screen.height));
            DebugLinesScript.Show("renderScale", urp.renderScale);
            for (int i = 0; i < Camera.allCamerasCount; i++)
            {
                var c = Camera.allCameras[i];
                DebugLinesScript.Show($"rect-{i}", c.rect);
                DebugLinesScript.Show($"pixelRect-{i}", c.pixelRect);
            }
        }
    }
}
