using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private float pitchReduction = 1f;
    [SerializeField] private AudioClip TitleScreenMusic;
    private bool PlayingMusic = true;

    private double songStartDspTime;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = TitleScreenMusic;
        GameManager.ToggleMusic += ToggleMusic;
    }

    private void Update()
    {
        if (PlayingMusic)
        {
            audioSource.pitch = Mathf.Min(1f, audioSource.pitch + Time.deltaTime * pitchReduction);
        }
        else 
        {
            audioSource.pitch = Mathf.Max(0f, audioSource.pitch - Time.deltaTime * pitchReduction);
        }
    }

    public void PlaySong() 
    {
        if (audioSource.clip == null) return;

        songStartDspTime = AudioSettings.dspTime;

        audioSource.pitch = 1f;
        PlayingMusic = true;
        audioSource.time = 0;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    public float SongTimeInMilliseconds() 
    {
        if (!audioSource.isPlaying) return 0f;

        // Current hardware time minus the start hardware time equals elapsed seconds
        double elapsedSeconds = AudioSettings.dspTime - songStartDspTime;

        // Convert to milliseconds
        return (float)(elapsedSeconds * 1000.0);
    }

    public float SongTimeInSeconds()
    {
        return audioSource.time;
    }

    private void ToggleMusic(bool toggle) 
    {
        PlayingMusic = toggle;
    }

    public void PlayMenuMusic() 
    {
        audioSource.clip = TitleScreenMusic;
        PlaySong();
    }
}
