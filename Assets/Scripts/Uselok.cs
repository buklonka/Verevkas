using UnityEngine;
using System.Collections;

public class Uzelok : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip dragSound;
    private AudioSource audioSource;

    [Header("Work Area Settings")]
    [SerializeField] private float workAreaWidth = 5f;
    [SerializeField] private float workAreaHeight = 5f;

    [Header("Scale Settings")]
    [SerializeField] private float scaleMultiplier = 1.5f;

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

    private bool isPlayingSound = false;
    private Vector3 lastPosition;

    void Start()
    {
        InitializeAudioSource();
        InitializeHighlight();
        InitializeCamera();
        InitializeWinController();
        InitializePositionAndScale();
    }

    void Update()
    {
        if (IsWinActive()) return;

        UpdateHighlight();
        HandleDragging();
    }

    void OnMouseEnter() { isMouseOver = true; }
    void OnMouseExit() { isMouseOver = false; }

    void OnMouseDown()
    {
        if (IsWinActive()) return;

        StartDragging();
    }

    void OnMouseUp()
    {
        StopDragging();
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StartCoroutine(EnableAudioAfterUserInteraction());
        }
    }

    private void InitializeHighlight()
    {
        if (highlight != null)
        {
            highlightRenderer = highlight.GetComponent<SpriteRenderer>();
            if (highlightRenderer != null)
            {
                highlightRenderer.color = transparentColor;
                highlight.SetActive(true);
            }
        }
    }

    private void InitializeCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            enabled = false;
            return;
        }

        CalculateWorkAreaBounds();
    }

    private void InitializeWinController()
    {
        winController = FindFirstObjectByType<WinTextController>();
        if (winController == null)
        {
            enabled = false;
        }
    }

    private void InitializePositionAndScale()
    {
        initialZ = transform.position.z;
        originalScale = transform.localScale;
        initialPosition = transform.position;
        lastPosition = transform.position;
    }

    private void UpdateHighlight()
    {
        if (highlightRenderer != null)
        {
            float targetAlpha = (isMouseOver && !isDragging && !isClicking) ? 1f : 0f;
            highlightRenderer.color = Color.Lerp(highlightRenderer.color, new Color(1, 1, 1, targetAlpha), Time.deltaTime / fadeDuration);
        }
    }

    private void HandleDragging()
    {
        if (isDragging)
        {
            Vector2 mousePosition2D = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition2D += new Vector2(offset.x, offset.y);

            mousePosition2D.x = Mathf.Clamp(mousePosition2D.x, minBounds.x, maxBounds.x);
            mousePosition2D.y = Mathf.Clamp(mousePosition2D.y, minBounds.y, maxBounds.y);

            transform.position = new Vector3(mousePosition2D.x, mousePosition2D.y, initialZ);

            if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
            {
                if (!isPlayingSound && dragSound != null && audioSource != null)
                {
                    StartCoroutine(PlayDragSoundWithCooldown());
                }
            }

            lastPosition = transform.position;
        }
    }

    private void StartDragging()
    {
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

    private void StopDragging()
    {
        isClicking = false;
        isDragging = false;
        transform.localScale = originalScale;
    }

    private void CalculateWorkAreaBounds()
    {
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

    public void ResetToInitialState()
    {
        transform.position = initialPosition;
        transform.localScale = originalScale;
        isDragging = false;
        isMouseOver = false;
        isClicking = false;
    }

    private bool IsWinActive()
    {
        return winController != null && winController.IsWinActive();
    }

    private IEnumerator EnableAudioAfterUserInteraction()
    {
        yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
        audioSource.PlayOneShot(hoverSound);
    }

    private IEnumerator PlayDragSoundWithCooldown()
    {
        isPlayingSound = true;

        if (dragSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dragSound);
        }

        yield return new WaitForSeconds(dragSound.length);
        yield return new WaitForSeconds(2f);

        isPlayingSound = false;
    }
}