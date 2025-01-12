using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class WinTextController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI winText; // Текст победы
    public TextMeshProUGUI scoreText; // Текст очков

    [Header("Particle System")]
    public ParticleSystem starEffect; // Эффект звёздочек

    [Header("Animation Settings")]
    public float fadeDuration = 2f; // Длительность появления текста
    public float starEffectDelay = 1.5f; // Задержка перед запуском звёздочек
    public float scoreIncreaseDuration = 1.5f; // Длительность анимации увеличения очков

    [Header("Score Settings")]
    public int winScore = 350; // Количество очков за победу

    private bool isWinActive = false; // Флаг, указывающий, активна ли победа
    private int currentScore = 0; // Текущее количество очков

    void Start()
    {
        // Загружаем сохранённые очки (если они есть)
        if (PlayerPrefs.HasKey("CurrentScore"))
        {
            currentScore = PlayerPrefs.GetInt("CurrentScore");
            UpdateScoreText();
        }

        // Выключаем текст и эффект звёздочек при старте
        if (winText != null)
            winText.gameObject.SetActive(false);

        if (starEffect != null)
        {
            starEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Подписываемся на событие "все верёвки зелёные"
        Svyaznoy.OnAllRopesGreen += OnWin;
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        Svyaznoy.OnAllRopesGreen -= OnWin;
    }

    // Метод, вызываемый при завершении игры
    private void OnApplicationQuit()
    {
        // Очищаем сохранённые очки
        PlayerPrefs.DeleteKey("CurrentScore");
        PlayerPrefs.Save();
    }

    // Метод, вызываемый при победе
    private void OnWin()
    {
        if (!isWinActive) // Если победа ещё не активна
        {
            isWinActive = true;
            ShowWinText();
            StartCoroutine(IncreaseScoreAnimation(currentScore, currentScore + winScore));
        }
    }

    // Метод для показа текста и эффектов победы
    private void ShowWinText()
    {
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            StartCoroutine(FadeInText());
        }

        if (starEffect != null)
        {
            StartCoroutine(PlayStarEffectWithDelay());
        }
    }

    // Корутина для плавного появления текста
    IEnumerator FadeInText()
    {
        float elapsedTime = 0f;
        Color textColor = winText.color;
        textColor.a = 0f;
        winText.color = textColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            winText.color = textColor;
            yield return null;
        }

        textColor.a = 1f;
        winText.color = textColor;
    }

    // Корутина для задержки перед запуском звёздочек
    IEnumerator PlayStarEffectWithDelay()
    {
        yield return new WaitForSeconds(starEffectDelay);
        starEffect.Play();
    }

    // Корутина для анимации увеличения очков
    IEnumerator IncreaseScoreAnimation(int startScore, int targetScore)
    {
        float elapsedTime = 0f;

        while (elapsedTime < scoreIncreaseDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / scoreIncreaseDuration);
            currentScore = (int)Mathf.Lerp(startScore, targetScore, progress);
            UpdateScoreText();
            yield return null;
        }

        currentScore = targetScore;
        UpdateScoreText();
    }

    // Метод для обновления текста очков
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Очки: " + currentScore;
        }
    }

    // Метод для сброса всей сцены с сохранением очков
    public void ResetSceneWithScore()
    {
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Метод для проверки, активна ли победа
    public bool IsWinActive()
    {
        return isWinActive;
    }
}