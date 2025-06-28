using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] mazeMusic;
    public AudioClip playButtonSound;

    [Range(0f,1f)]
    public float musicVolume = 0.2f;

    private AudioSource audioSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // Если уже есть AudioManager, не дублируем его
        if (FindObjectsOfType<AudioManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = musicVolume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Когда загрузилась сцена лабиринта — запускаем музыку
        if (scene.name == "SampleScene")  
        {
            if (!audioSource.isPlaying)
            {
                int index = Random.Range(0, mazeMusic.Length);
                audioSource.clip = mazeMusic[index];
                audioSource.Play();
            }
        }
        else
        {
            // В любой другой сцене (например, главное меню) останавливаем
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    // Вызывай этот метод из кнопки
    public void PlayButtonClick()
    {
        if (playButtonSound != null)
            sfxSource.PlayOneShot(playButtonSound);
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
