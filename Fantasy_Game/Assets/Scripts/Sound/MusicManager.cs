using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip dragonBossMusic;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float musicVolume = 0.35f;

    private Coroutine fadeCoroutine;

    // Sets up the music manager and starts the default music
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }
    }

    // Starts the default ambient music when the scene begins
    private void Start()
    {
        PlayDefaultMusic();
    }

    // Plays the normal ambient music
    public void PlayDefaultMusic()
    {
        ChangeMusic(defaultMusic);
    }

    // Plays the dragon boss music
    public void PlayDragonBossMusic()
    {
        ChangeMusic(dragonBossMusic);
    }

    // Changes the current music with a fade transition
    private void ChangeMusic(AudioClip newClip)
    {
        if (musicSource == null || newClip == null)
        {
            return;
        }

        if (musicSource.clip == newClip && musicSource.isPlaying)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToNewMusic(newClip));
    }

    // Fades out the current track, swaps clip, then fades the new track in
    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }
}