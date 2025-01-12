using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class WinTextController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem starEffect;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float starEffectDelay = 1.5f;
    [SerializeField] private float scoreIncreaseDuration = 1.5f;

    [Header("Score Settings")]
    [SerializeField] private int winScore = 350;

    private bool isWinActive = false;
    private int currentScore = 0;

    void Start()
    {
        LoadScore();
        InitializeUI();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnApplicationQuit()
    {
        ClearSavedScore();
    }

    private void LoadScore()
    {
        if (PlayerPrefs.HasKey("CurrentScore"))
        {
            currentScore = PlayerPrefs.GetInt("CurrentScore");
            UpdateScoreText();
        }
    }

    private void InitializeUI()
    {
        if (winText != null)
            winText.gameObject.SetActive(false);

        if (starEffect != null)
        {
            starEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void SubscribeToEvents()
    {
        Svyaznoy.OnAllRopesGreen += OnWin;
    }

    private void UnsubscribeFromEvents()
    {
        Svyaznoy.OnAllRopesGreen -= OnWin;
    }

    private void ClearSavedScore()
    {
        PlayerPrefs.DeleteKey("CurrentScore");
        PlayerPrefs.Save();
    }

    private void OnWin()
    {
        if (!isWinActive)
        {
            isWinActive = true;
            ShowWinText();
            StartCoroutine(IncreaseScoreAnimation(currentScore, currentScore + winScore));
        }
    }

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

    private IEnumerator FadeInText()
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

    private IEnumerator PlayStarEffectWithDelay()
    {
        yield return new WaitForSeconds(starEffectDelay);
        starEffect.Play();
    }

    private IEnumerator IncreaseScoreAnimation(int startScore, int targetScore)
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

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Очки: " + currentScore;
        }
    }

    public void ResetSceneWithScore()
    {
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsWinActive()
    {
        return isWinActive;
    }
}