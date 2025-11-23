using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedRenderTextureResizer : MonoBehaviour
{
    public Camera targetCamera;
    public RenderTexture targetTexture;

    [Tooltip("Resize RT at start")]
    public bool resizeAtStart = true;

    void Start()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (targetTexture == null)
        {
            Debug.LogError("Assign a RenderTexture!");
            return;
        }

        if (resizeAtStart)
            ResizeRenderTexture();
    }

    void ResizeRenderTexture()
    {
        int width = Screen.width;
        int height = Screen.height;

        // Validate sizes
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        // Only resize if needed
        if (targetTexture.width != width || targetTexture.height != height)
        {
            // Save old settings or provide defaults
            RenderTextureFormat format = targetTexture.format != RenderTextureFormat.Default ? targetTexture.format : RenderTextureFormat.ARGB32;
            int depth = targetTexture.depth; // 0 if no depth needed
            int aa = targetTexture.antiAliasing > 0 ? targetTexture.antiAliasing : 1;

            // Release old RT
            targetTexture.Release();

            targetTexture.width = width;
            targetTexture.height = height;
            targetTexture.format = format;
            targetTexture.depth = depth;
            targetTexture.antiAliasing = aa;

            targetTexture.Create();

            // Reassign to camera
            targetCamera.targetTexture = targetTexture;
        }
    }
}
