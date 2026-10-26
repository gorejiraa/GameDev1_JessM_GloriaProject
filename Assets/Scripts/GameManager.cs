using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MusicController Music;
    public Metronome Metronome;
    public Choreographer Choreographer;
    public Judge Judge;
    public LightingManager LightingManager;
    public UIManager UIManager;
    public IndicatorManager IndicatorManager;
    public DialogueManager DialogueManager;

    public static event Action<bool> OnToggleMusic;
    public static event Action ResetGameState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        
    }

    private void Start()
    {
        EnemyEncounter.SendEncounterDetails += GetEnemyDetails;
        EnemyEncounter.OnEnemyHealthEmpty += WinGame;
        PlayerStatus.OnPlayerHealthEmpty += LoseGame;

        UIManager.ToggleMainMenu(true);
    }

    public void StartGameplay() 
    {
        EnemyEncounter enemy = GameObject.FindAnyObjectByType<EnemyEncounter>();
        enemy.BeginEncounter();

        Music.PlaySong();
        Metronome.ResetValues();
        Metronome.ToggleGameplay(true);
        UIManager.ToggleGameplayUI(true);
        UIManager.ToggleMainMenu(false);
        IndicatorManager.ToggleIndicators(true);
    }

    public void StartButEvil() 
    {
        EnemyEncounter enemy = GameObject.FindAnyObjectByType<EnemyEncounter>();
        Track evilTrack = Instantiate(Resources.Load<Track>("Tracks/TitleSong"));
        evilTrack.BeatsInBar = 8;

        GetEnemyDetails(evilTrack, enemy.TimingWindow);

        Music.PlaySong();
        Metronome.ResetValues();
        Metronome.ToggleGameplay(true);
        UIManager.ToggleGameplayUI(true);
        UIManager.ToggleMainMenu(false);
    }

    public void GetEnemyDetails(Track track, float timingWindow) 
    {
        Debug.Log("Ran get enemy details...");

        Music.audioSource.clip = track.Song;
        Metronome.ResetValues(track);
        Choreographer.ResetValues(track);
        Metronome.TimingWindowMargin = timingWindow;
        LightingManager.SetStartBattleBeat(track.GameplayStartBeat);
        DialogueManager.AddAllTimedDialogue(track.Dialogue);
    }

    public void LoseGame() 
    {
        OnToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);

        UIManager.ShowLossPanel();
    }

    public void WinGame() 
    {
        OnToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);

        UIManager.ShowWinPanel();
    }

    public void PauseGameplay() 
    {
        OnToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);
    }

    public void ResumeGameplay() 
    {
        OnToggleMusic?.Invoke(true);
        Metronome.ToggleGameplay(true);
    }

    public void RestartGame() 
    {
        ResetGameState?.Invoke();

        StartGameplay();
    }

    public void ReturnToMenu() 
    {
        ResetGameState?.Invoke();
        UIManager.ToggleGameplayUI(false);
        UIManager.ToggleMainMenu(true);
        IndicatorManager.ToggleIndicators(false);
    }

    public void QuitGame() 
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
