using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;

    // We declare these public so it's clear what sounds this manager uses,
    // but we will load them from Resources in Awake.
    public AudioClip buttonClickSound;
    public AudioClip ropeCollisionSound;
    public AudioClip levelWinSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        // Load clips from the Resources/Sound folder
        buttonClickSound = Resources.Load<AudioClip>("Sound/01_click");
        ropeCollisionSound = Resources.Load<AudioClip>("Sound/01_click"); // Placeholder
        levelWinSound = Resources.Load<AudioClip>("Sound/Untitled (1)"); // Placeholder for win sound

        if (buttonClickSound == null || ropeCollisionSound == null || levelWinSound == null)
        {
            Debug.LogError("SoundManager: Failed to load one or more audio clips from Resources/Sound/");
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayRopeCollision()
    {
        // To avoid sound spam, we can add a small cooldown here if needed.
        // For now, just play the sound.
        if (ropeCollisionSound != null)
        {
            audioSource.PlayOneShot(ropeCollisionSound, 0.5f); // Play at half volume
        }
    }

    public void PlayLevelWin()
    {
        if (levelWinSound != null)
        {
            audioSource.PlayOneShot(levelWinSound);
        }
    }
}
