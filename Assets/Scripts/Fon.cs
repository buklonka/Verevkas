using UnityEngine;

public class ScaleBackground : MonoBehaviour
{
    void Start()
    {
        ScaleBackgroundToScreen();
    }

    private void ScaleBackgroundToScreen()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            enabled = false;
            return;
        }

        Vector2 screenSize = CalculateScreenSize();
        Vector2 spriteSize = GetSpriteSize(spriteRenderer);
        ScaleSprite(screenSize, spriteSize);
    }

    private Vector2 CalculateScreenSize()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;
        return new Vector2(screenWidth, screenHeight);
    }

    private Vector2 GetSpriteSize(SpriteRenderer spriteRenderer)
    {
        return spriteRenderer.sprite.bounds.size;
    }

    private void ScaleSprite(Vector2 screenSize, Vector2 spriteSize)
    {
        float scaleX = screenSize.x / spriteSize.x;
        float scaleY = screenSize.y / spriteSize.y;
        transform.localScale = new Vector3(scaleX, scaleY, 1);
    }
}