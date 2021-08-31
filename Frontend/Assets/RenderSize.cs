using System.Linq;
using UnityEngine;

public class RenderSize : MonoBehaviour
{
    public Camera[] Cameras;
    bool resolutionChangeAttempted = false;

    // 16:9  = 1.77 / 0.5625
    // 16:10 = 1.60 / 0.6250
    private void Awake()
    {
        Cameras = FindObjectsOfType<Camera>();
        SetCameraViewports();
    }

    void SetCameraViewports()
    {
        // Target resolution: 16:9, by far the most common on Steam survey. All rendering should be kept inside this.

        // set viewport to 16:9 or 16:10, whatever is closest to actual resolution. Everything else will get black bars left/right.
        // Create a 16:10 viewport as that is the by far must occuring on Steam stats.
        const float Ratio16_9 = 16.0f / 9.0f;
        const float Ratio16_10 = 16.0f / 10.0f;
        float currentRatio = Screen.width / (float)Screen.height;
        float distanceTo16_9 = Mathf.Abs(Ratio16_9 - currentRatio);
        float distanceTo16_10 = Mathf.Abs(Ratio16_10 - currentRatio);

        float chosenWidth = distanceTo16_9 < distanceTo16_10 ?
            Screen.height * Ratio16_9 : // Closest to 16:9
            Screen.height * Ratio16_10; // Closest to 16:10

        // Lower resolution for 4K screens
        if (chosenWidth > 3000 && !resolutionChangeAttempted)
        {
            resolutionChangeAttempted = true;

            chosenWidth /= 2;
            var lowerRes = Screen.resolutions.Where(res => res.width == chosenWidth).FirstOrDefault();
            if (lowerRes.width != 0)
            {
                Debug.Log($"Lowering resolution since it is > 3000: {lowerRes}");
                Screen.SetResolution(lowerRes.width, lowerRes.height, fullscreen: true);
                // Resolution change only takes effect in next frame
                return;
            }
        }

        foreach (var cam in Cameras)
        {
            bool isAlreadyChosenWidth = Mathf.Abs(cam.pixelRect.width - chosenWidth) < 1f;
            if (!isAlreadyChosenWidth)
            {
                var newPixelRect = new Rect(0, 0, chosenWidth, Screen.height);
                Debug.Log($"Camera {cam.name}:");
                Debug.Log($"- current screen resolution: {Screen.currentResolution}, current window resolution: w={Screen.width}, h={Screen.height}");
                Debug.Log($"- fullScreen: {Screen.fullScreen}, fullScreenMode: {Screen.fullScreenMode}");
                Debug.Log($"- changing viewport from {cam.pixelRect} to {newPixelRect}");
                cam.pixelRect = newPixelRect;
            }
        }
    }

    private void Update()
    {
        SetCameraViewports();
    }
}
