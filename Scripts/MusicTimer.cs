using UnityEngine;

public class MusicTimer : MonoBehaviour
{
    public AudioSource musicAudioSource;
    public float startTime = 65f;

    private float timer = 0f;
    private bool musicStarted = false;

    void Start()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }

    void Update()
    {
        if (!musicStarted)
        {
            timer += Time.deltaTime;

            if (timer >= startTime)
            {
                StartMusic();
            }
        }
    }

    void StartMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Play();
            musicStarted = true;
        }
    }
}