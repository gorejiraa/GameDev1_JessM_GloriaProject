using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MusicController Music;
    public Metronome Metronome;
    public Choreographer Choreographer;
    public Judge Judge;
    public LightingManager LightingManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        EnemyEncounter.SendEncounterDetails += GetEnemyDetails;
    }

    public void StartGameplay() 
    {
        EnemyEncounter enemy = GameObject.FindAnyObjectByType<EnemyEncounter>();
        enemy.BeginEncounter();

        Music.PlaySong();
        Metronome.ResetValues();
        Metronome.ToggleGameplay(true);
    }

    public void GetEnemyDetails(Track track, float timingWindow) 
    {
        Music.audioSource.clip = track.Song;
        Metronome.ResetValues(track);
        Choreographer.ResetValues(track.GameplayStartBeat);
        Metronome.TimingWindowMargin = timingWindow;
        LightingManager.SetStartBattleBeat(track.GameplayStartBeat);
    }
}
