using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderSize : MonoBehaviour
{
    public static bool AllowDownsizingRenderScale = false;

    public Camera[] Cameras;
    bool resolutionChangeAttempted = false;
    float viewPortChosenWidthAttempt;

    private int _currentViewportWidth = -1;

    UniversalRenderPipelineAsset urp;

    private void Awake()
    {
        Cameras = FindObjectsOfType<Camera>();
        SetCameraViewports();
        urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1;
        timeAllowRenderScaleChange = Time.time + 3;
    }

    void SetCameraViewports()
    {
        // Force game view inside 16:10, adding black borders as necessary
        float desiredViewportWidth = (int)(Screen.height * 1.6f);
        float unusableWidth = Screen.width - desiredViewportWidth;

        // Lower resolution for 4K screens
        if (desiredViewportWidth > 3000 && !resolutionChangeAttempted && Screen.fullScreen)
        {
            resolutionChangeAttempted = true;

            desiredViewportWidth /= 2;
            var lowerRes = Screen.resolutions.Where(res => res.width == desiredViewportWidth).FirstOrDefault();
            if (lowerRes.width != 0)
            {
                Debug.Log($"Lowering resolution since it is > 3000: {lowerRes}");
                Screen.SetResolution(lowerRes.width, lowerRes.height, fullscreen: true);
                // Resolution change only takes effect in next frame
                return;
            }
        }

        //if (_currentViewportWidth == desiredViewportWidth)
        //    return;

        for (int i = 0; i < Cameras.Length; ++i)
        {
            var cam = Cameras[i];
            bool isAlreadyChosenWidth = Mathf.Abs(cam.pixelRect.width - desiredViewportWidth) < 1f;
            if (!isAlreadyChosenWidth)
            {
                // WebGL was going crazy trying to resize every frame.
                // In case resize fails don't keep trying.
                //bool widthWasAlreadyAttempted = desiredViewportWidth == viewPortChosenWidthAttempt;
                //if (!widthWasAlreadyAttempted)
                {
                    var newPixelRect = new Rect(unusableWidth / 2, 0, desiredViewportWidth, Screen.height);
                    Debug.Log($"Camera {cam.name}:");
                    Debug.Log($"- current screen resolution: {Screen.currentResolution}, current window resolution: w={Screen.width}, h={Screen.height}");
                    Debug.Log($"- fullScreen: {Screen.fullScreen}, fullScreenMode: {Screen.fullScreenMode}");
                    Debug.Log($"- changing viewport from {cam.pixelRect} to {newPixelRect}");
                    cam.pixelRect = newPixelRect;

                    viewPortChosenWidthAttempt = desiredViewportWidth;
                }
            }
        }
    }

    bool showStats;
    float timeWithBadFps;
    double exponentialAvg;
    float timeAllowRenderScaleChange;

    private void Update()
    {
        SetCameraViewports();
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
            DebugLinesScript.Show("viewport", Cameras[0].pixelRect);
            DebugLinesScript.Show("renderScale", urp.renderScale);
        }
    }
}
