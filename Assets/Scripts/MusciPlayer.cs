using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance; // Статическая переменная для хранения единственного экземпляра
    private AudioSource audioSource;

    void Awake()
    {
        // Проверяем, существует ли уже экземпляр MusicPlayer
        if (instance == null)
        {
            instance = this; // Делаем этот объект единственным экземпляром
            DontDestroyOnLoad(gameObject); // Запрещаем уничтожение объекта при загрузке новой сцены
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            // Если экземпляр уже существует, уничтожаем этот объект
            Destroy(gameObject);
        }
    }

    // Метод для изменения громкости музыки
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume); // Громкость должна быть в диапазоне [0, 1]
        }
    }

    // Метод для включения/выключения музыки
    public void ToggleMusic(bool isOn)
    {
        if (audioSource != null)
        {
            audioSource.mute = !isOn;
        }
    }
}