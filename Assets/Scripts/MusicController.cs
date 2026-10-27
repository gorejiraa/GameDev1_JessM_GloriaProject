using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private float pitchReduction = 1f;
    private bool PlayingMusic = true;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
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

        Debug.Log("Playing song");

        audioSource.pitch = 1f;
        PlayingMusic = true;
        audioSource.time = 0;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    public float SongTimeInMilliseconds() 
    {
        return audioSource.time * 1000;
    }

    public float SongTimeInSeconds()
    {
        return audioSource.time;
    }

    private void ToggleMusic(bool toggle) 
    {
        PlayingMusic = toggle;
    }
}
