using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private void Start()
    {
        Camera cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (cam == null || sr.sprite == null) return;

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        Vector3 scale = transform.localScale;
        scale.x = screenWidth / spriteSize.x;
        scale.y = screenHeight / spriteSize.y;

        transform.localScale = scale;
    }
}