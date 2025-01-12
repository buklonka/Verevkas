using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;
    private AudioSource audioSource;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private float fadeOutDuration = 3f;

    private const float MinVolume = 0f;
    private const float MaxVolume = 1f;

    void Awake()
    {
        InitializeSingleton();
        InitializeAudioSource();
        StartCoroutine(PlayMusicWithFade());
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing!");
            enabled = false;
        }
    }

    private IEnumerator PlayMusicWithFade()
    {
        while (true)
        {
            yield return FadeIn();
            yield return WaitUntilNearEnd();
            yield return FadeOut();
            RestartMusic();
        }
    }

    private IEnumerator FadeIn()
    {
        if (audioSource == null) yield break;

        audioSource.volume = MinVolume;
        audioSource.Play();

        yield return ChangeVolumeOverTime(MinVolume, MaxVolume, fadeInDuration);
    }

    private IEnumerator FadeOut()
    {
        if (audioSource == null) yield break;

        yield return ChangeVolumeOverTime(audioSource.volume, MinVolume, fadeOutDuration);
        audioSource.Stop();
    }

    private IEnumerator WaitUntilNearEnd()
    {
        if (audioSource == null || audioSource.clip == null) yield break;

        float timeLeft = audioSource.clip.length - audioSource.time;
        while (timeLeft > fadeOutDuration)
        {
            yield return null;
            timeLeft = audioSource.clip.length - audioSource.time;
        }
    }

    private IEnumerator ChangeVolumeOverTime(float startVolume, float targetVolume, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private void RestartMusic()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp(volume, MinVolume, MaxVolume);
        }
    }

    public void ToggleMusic(bool isOn)
    {
        if (audioSource != null)
        {
            audioSource.mute = !isOn;
        }
    }
}