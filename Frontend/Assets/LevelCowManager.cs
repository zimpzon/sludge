using UnityEngine;

public class LevelCowManager : MonoBehaviour
{
    private SpriteRenderer cowSprite;

    private void Awake()
    {
        cowSprite = GetComponent<SpriteRenderer>();
    }
}
