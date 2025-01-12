using UnityEngine;

public class ScaleBackground : MonoBehaviour
{
    void Start()
    {
        // Масштабируем фон
        ScaleBackgroundToScreen();
    }

    // Метод для масштабирования фона
    private void ScaleBackgroundToScreen()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Получаем размеры экрана в мировых координатах
            float screenHeight = Camera.main.orthographicSize * 2;
            float screenWidth = screenHeight * Screen.width / Screen.height;

            // Получаем размеры спрайта
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;

            // Масштабируем спрайт
            transform.localScale = new Vector3(screenWidth / spriteWidth, screenHeight / spriteHeight, 1);
        }
    }
}