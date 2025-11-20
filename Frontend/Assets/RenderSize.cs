using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Helper component to store original canvas settings
public class OriginalCanvasSettings : MonoBehaviour
{
    public Vector2 originalReferenceResolution;
    public float originalMatchMode;
}

public class RenderSize : MonoBehaviour
{
    public Camera Camera;

    // Target aspect ratio for the game (16:9)
    private const float targetAspectRatio = 16.0f / 9.0f;

    UniversalRenderPipelineAsset urp;

    private void Awake()
    {
        Camera = Camera.main;
        urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1;
        Debug.Log("TODO: lower render scale on 4K etc?");

        // Set up letterboxing for proper aspect ratio handling
        ApplyLetterboxing();
    }

    private void ApplyLetterboxing()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        Rect cameraRect;

        if (Mathf.Approximately(currentAspectRatio, targetAspectRatio))
        {
            // Perfect match, use full screen
            cameraRect = new Rect(0, 0, 1, 1);
        }
        else if (currentAspectRatio > targetAspectRatio)
        {
            // Screen is wider than target (e.g., ultrawide), add pillarboxing (side bars)
            float targetWidth = targetAspectRatio / currentAspectRatio;
            float offsetX = (1.0f - targetWidth) / 2.0f;
            cameraRect = new Rect(offsetX, 0, targetWidth, 1);
        }
        else
        {
            // Screen is taller than target (e.g., 16:10), add letterboxing (top/bottom bars)
            float targetHeight = currentAspectRatio / targetAspectRatio;
            float offsetY = (1.0f - targetHeight) / 2.0f;
            cameraRect = new Rect(0, offsetY, 1, targetHeight);
        }

        // Apply the same rect to ALL cameras (main camera, overlay cameras, UI cameras, etc.)
        for (int i = 0; i < Camera.allCamerasCount; i++)
        {
            var cam = Camera.allCameras[i];
            cam.rect = cameraRect;
        }

        // Handle Screen Space Overlay canvases - they don't use camera rects
        ApplyCanvasLetterboxing(cameraRect);

        Debug.Log($"Applied aspect ratio correction to {Camera.allCamerasCount} cameras: Screen {Screen.width}x{Screen.height} ({currentAspectRatio:F3}), Target {targetAspectRatio:F3}, Camera Rect: {cameraRect}");
    }

    private void ApplyCanvasLetterboxing(Rect cameraRect)
    {
        var canvases = FindObjectsOfType<Canvas>();

        foreach (var canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For 2D games, switching to camera mode can be tricky
                // Let's try a different approach - adjust the CanvasScaler to respect letterboxing
                var canvasScaler = canvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
                }

                // Store original settings to restore if needed
                if (!canvas.gameObject.TryGetComponent<OriginalCanvasSettings>(out var originalSettings))
                {
                    originalSettings = canvas.gameObject.AddComponent<OriginalCanvasSettings>();
                    originalSettings.originalReferenceResolution = canvasScaler.referenceResolution;
                    originalSettings.originalMatchMode = canvasScaler.matchWidthOrHeight;
                }

                // Configure the CanvasScaler to handle letterboxing while staying in overlay mode
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                // Adjust reference resolution based on letterboxing
                if (cameraRect.width < 1.0f) // Pillarboxing (ultrawide)
                {
                    canvasScaler.referenceResolution = new Vector2(800 / cameraRect.width, 450);
                    canvasScaler.matchWidthOrHeight = 1.0f; // Match height
                }
                else if (cameraRect.height < 1.0f) // Letterboxing (16:10 -> 16:9)
                {
                    canvasScaler.referenceResolution = new Vector2(800, 450 / cameraRect.height);
                    canvasScaler.matchWidthOrHeight = 0.0f; // Match width
                }
                else // Perfect match
                {
                    canvasScaler.referenceResolution = new Vector2(800, 450);
                    canvasScaler.matchWidthOrHeight = 1.0f;
                }

                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            }
        }
    }

    bool showStats;
    double exponentialAvg;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Update()
    {
        // Check for resolution changes and reapply letterboxing
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyLetterboxing();
        }

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
