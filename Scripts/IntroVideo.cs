using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class IntroVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public RawImage rawImage;
    public float skipDelay = 1f;
    public bool isSkip = false;

    [Header("Настройки звука")]
    public float volume = 1f;
    public bool muteOnSkip = true;

    [Header("Настройки исчезновения")]
    public float fadeOutDuration = 1.5f;
    public bool fadeOutAtEnd = true;

    private bool isSkipping = false;
    private bool isFading = false;


    void Start()
    {
        Time.timeScale = 0f;

        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (rawImage == null) rawImage = GetComponent<RawImage>();

        audioSource.volume = volume;

        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.loopPointReached += OnVideoFinished;

        StartCoroutine(EnableSkipAfterDelay());
    }

    IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSecondsRealtime(skipDelay);
    }

    void Update()
    {
        if (Input.anyKeyDown && isSkip)
        {
            SkipVideo();
        }

        if (isSkipping)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.unscaledDeltaTime * 5f);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (fadeOutAtEnd && !isFading)
        {
            StartCoroutine(FadeOutAndLoad());
        }
        else if (!fadeOutAtEnd)
        {
            EndCutscene();
        }
    }

    IEnumerator FadeOutAndLoad()
    {
        isFading = true;
        float elapsedTime = 0f;
        Color startColor = rawImage.color;
        float startVolume = audioSource.volume;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            rawImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);

            yield return null;
        }

        EndCutscene();
    }

    void EndCutscene()
    {
        Time.timeScale = 1f;

        gameObject.SetActive(false);
    }

    void SkipVideo()
    {
        if (isSkipping) return;
        isSkipping = true;

        if (muteOnSkip)
        {
            StartCoroutine(FadeOutAndLoad());
        }
        else
        {
            EndCutscene();
        }
    }
}