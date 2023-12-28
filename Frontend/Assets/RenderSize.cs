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
        SetCameraViewports();
        urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1;
        Debug.Log("TODO: lower render scale on 4K etc?");
    }

    void SetCameraViewports()
    {
        // TODO: fill better than black border?
        // 16:9 is the most common aspect ratio (Steam survey 2023)
        const float DesiredAspectRatio = 9f / 16f; // 0.5625
        float desiredViewportWidth = Screen.height / DesiredAspectRatio;
        float unusableWidth = Screen.width - desiredViewportWidth;
        Camera.pixelRect = new Rect(unusableWidth / 2, 0, desiredViewportWidth, Screen.height);
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
            DebugLinesScript.Show("viewport", Camera.pixelRect);
            DebugLinesScript.Show("renderScale", urp.renderScale);
        }
    }
}
