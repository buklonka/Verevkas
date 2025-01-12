using UnityEngine;
using System.Collections;

public class Uzelok : MonoBehaviour
{
    [Header("Highlight Settings")]
    public GameObject highlight;
    public float fadeDuration = 0.2f;

    [Header("Sound Settings")]
    public AudioClip hoverSound;
    public AudioClip dragSound;
    private AudioSource audioSource;

    [Header("Work Area Settings")]
    public float workAreaWidth = 5f;
    public float workAreaHeight = 5f;

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.5f;

    private SpriteRenderer highlightRenderer;
    private Color transparentColor = new Color(1, 1, 1, 0);

    private bool isDragging = false;
    private Vector3 offset;
    private bool isMouseOver = false;
    private bool isClicking = false;

    private Camera mainCamera;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    private float initialZ;
    private Vector3 originalScale;
    private Vector3 initialPosition;

    private WinTextController winController;

    private bool isPlayingSound = false; // Флаг для отслеживания воспроизведения звука
    private Vector3 lastPosition; // Последняя позиция объекта

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Разблокировка аудио при первом клике (для WebGL)
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StartCoroutine(EnableAudioAfterUserInteraction());
        }

        if (highlight != null)
        {
            highlightRenderer = highlight.GetComponent<SpriteRenderer>();
            if (highlightRenderer != null)
            {
                highlightRenderer.color = transparentColor;
                highlight.SetActive(true);
            }
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        winController = FindFirstObjectByType<WinTextController>();
        if (winController == null)
        {
            Debug.LogError("WinTextController not found!");
        }

        CalculateWorkAreaBounds();
        initialZ = transform.position.z;
        originalScale = transform.localScale;
        initialPosition = transform.position;

        lastPosition = transform.position; // Инициализация последней позиции
    }

    void Update()
    {
        if (winController != null && winController.IsWinActive())
        {
            if (isDragging)
            {
                isDragging = false;
                transform.localScale = originalScale;
            }
            return;
        }

        if (highlightRenderer != null)
        {
            float targetAlpha = (isMouseOver && !isDragging && !isClicking) ? 1f : 0f;
            highlightRenderer.color = Color.Lerp(highlightRenderer.color, new Color(1, 1, 1, targetAlpha), Time.deltaTime / fadeDuration);
        }

        if (isDragging)
        {
            Vector2 mousePosition2D = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition2D += new Vector2(offset.x, offset.y);

            mousePosition2D.x = Mathf.Clamp(mousePosition2D.x, minBounds.x, maxBounds.x);
            mousePosition2D.y = Mathf.Clamp(mousePosition2D.y, minBounds.y, maxBounds.y);

            transform.position = new Vector3(mousePosition2D.x, mousePosition2D.y, initialZ);

            // Проверка, движется ли объект
            if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
            {
                if (!isPlayingSound && dragSound != null && audioSource != null)
                {
                    StartCoroutine(PlayDragSoundWithCooldown());
                }
            }

            lastPosition = transform.position; // Обновление последней позиции
        }
    }

    void OnMouseEnter() { isMouseOver = true; }
    void OnMouseExit() { isMouseOver = false; }

    void OnMouseDown()
    {
        if (winController != null && winController.IsWinActive())
        {
            return;
        }

        isClicking = true;
        isDragging = true;
        offset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
        initialZ = transform.position.z;

        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        transform.localScale = originalScale * scaleMultiplier;
    }

    void OnMouseUp()
    {
        isClicking = false;
        isDragging = false;
        transform.localScale = originalScale;
    }

    void CalculateWorkAreaBounds()
    {
        if (mainCamera == null) return;

        Vector3 cameraCenter = mainCamera.transform.position;
        minBounds = new Vector2(cameraCenter.x - workAreaWidth / 2, cameraCenter.y - workAreaHeight / 2);
        maxBounds = new Vector2(cameraCenter.x + workAreaWidth / 2, cameraCenter.y + workAreaHeight / 2);

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            float colliderWidth = collider.bounds.extents.x;
            float colliderHeight = collider.bounds.extents.y;
            minBounds.x += colliderWidth;
            minBounds.y += colliderHeight;
            maxBounds.x -= colliderWidth;
            maxBounds.y -= colliderHeight;
        }
    }

    // Метод для сброса узелка в начальное состояние
    public void ResetToInitialState()
    {
        transform.position = initialPosition;
        transform.localScale = originalScale;
        isDragging = false;
        isMouseOver = false;
        isClicking = false;
    }

    IEnumerator EnableAudioAfterUserInteraction()
    {
        yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
        audioSource.PlayOneShot(hoverSound); // Воспроизведение тестового звука
    }

    IEnumerator PlayDragSoundWithCooldown()
    {
        isPlayingSound = true; // Устанавливаем флаг, что звук воспроизводится

        if (dragSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dragSound);
        }

        yield return new WaitForSeconds(dragSound.length); // Ждем окончания звука

        yield return new WaitForSeconds(2f); // Задержка 2 секунды

        isPlayingSound = false; // Сбрасываем флаг
    }
}