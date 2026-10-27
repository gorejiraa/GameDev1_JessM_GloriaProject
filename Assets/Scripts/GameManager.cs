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

    public OffsetCalculator OffsetCalculator;
    public int GeneralOffset = 0;

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
        EnemyEncounter.SendEncounterDetails += SetTrackDetails;
        EnemyEncounter.OnEnemyHealthEmpty += WinGame;
        PlayerStatus.OnPlayerHealthEmpty += LoseGame;
        OffsetCalculator.Complete += OnCompleteOffsetCalculation;

        UIManager.ToggleMainMenu(true);
        OffsetCalculator.gameObject.SetActive(false);
    }

    public void StartButton() 
    {
        EnemyEncounter enemy = GameObject.FindAnyObjectByType<EnemyEncounter>();
        enemy.BeginEncounter();

        StartGameplay();
        EnableGameplayUI();
    }

    public void StartGameplay() 
    {
        Music.PlaySong();
        Metronome.ToggleGameplay(true);
    }

    public void EnableGameplayUI() 
    {
        UIManager.ToggleGameplayUI(true);
        UIManager.ToggleMainMenu(false);
        IndicatorManager.ToggleIndicators(true);
    }

    public void StartButEvil() 
    {
        EnemyEncounter enemy = GameObject.FindAnyObjectByType<EnemyEncounter>();
        Track evilTrack = Instantiate(Resources.Load<Track>("Tracks/TitleSong"));
        evilTrack.BeatsInBar = 8;

        SetTrackDetails(evilTrack, enemy.TimingWindow);

        StartGameplay();
        EnableGameplayUI();
    }

    public void SetTrackDetails(Track track, float timingWindow) 
    {
        Music.audioSource.clip = track.Song;
        Metronome.ResetValues(track, GeneralOffset);
        Choreographer.ResetValues(track);
        Metronome.TimingWindowMargin = timingWindow;
        LightingManager.SetStartBattleBeat(track.GameplayStartBeat);
        DialogueManager.AddAllTimedDialogue(track.Dialogue);
    }

    public void BeginOffsetCalculation() 
    {
        OffsetCalculator.gameObject.SetActive(true);
        OffsetCalculator.Reset();

        Track track = OffsetCalculator.CalculationTrack;

        SetTrackDetails(track, 100);
        UIManager.ToggleOffsetCalculationPanel(true);
        UIManager.ToggleDisplayOffsetCalculation(false);
        StartGameplay();
    }

    public void OnCompleteOffsetCalculation() 
    {
        GeneralOffset = OffsetCalculator.GetOffsetAverageInt();
        OffsetCalculator.gameObject.SetActive(false);
        Music.audioSource?.Stop();
        Metronome.ToggleGameplay(false);
        UIManager.ToggleDisplayOffsetCalculation(true);
    }

    public void ExitOffsetCalculation() 
    {
        UIManager.ToggleOffsetCalculationPanel(false);
        ReturnToMenu();
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

        StartButton();
    }

    public void ReturnToMenu() 
    {
        ResetGameState?.Invoke();
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
