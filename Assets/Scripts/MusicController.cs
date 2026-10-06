using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySong() 
    {
        if (audioSource.clip == null) return;
        
        audioSource.time = 0;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    public float SongTimeInMilliseconds() 
    {
        return audioSource.time * 1000;
    }
}
