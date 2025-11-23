using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class QuadFitToScreen2D : MonoBehaviour
{
    public Camera cameraToMatch;

    void Update()
    {
        if (cameraToMatch == null) cameraToMatch = Camera.main;
        if (cameraToMatch == null) return;

        if (!cameraToMatch.orthographic)
        {
            Debug.LogWarning("QuadFitToScreen2D only supports orthographic cameras.");
            return;
        }

        // Quad scale = camera size in world units
        float height = cameraToMatch.orthographicSize * 2f;
        float width = height * cameraToMatch.aspect;

        transform.localScale = new Vector3(width, height, 1f);

        // Position at Z=0, centered on camera X/Y
        transform.position = new Vector3(cameraToMatch.transform.position.x,
                                         cameraToMatch.transform.position.y,
                                         0f);

        // No rotation needed
        transform.rotation = Quaternion.identity;
    }
}
