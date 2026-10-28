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
    public TutorialManager TutorialManager;
    public SongSelectMenu SongSelectMenu;

    public PlayerStatus PlayerStatus;
    public EnemyEncounter Enemy;

    public static event Action<bool> ToggleMusic;
    public static event Action ResetGameState;
    public static event Action EndBattle;

    public OffsetCalculator OffsetCalculator;
    public int GeneralOffset = 0;

    private Track currentTrack;

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
        EnemyEncounter.OnEnemyHealthEmpty += WinGame;
        PlayerStatus.OnPlayerHealthEmpty += LoseGame;
        OffsetCalculator.Complete += OnCompleteOffsetCalculation;

        LoadSettings();

        UIManager.ToggleMainMenu(true);
        OffsetCalculator.gameObject.SetActive(false);
        TutorialManager.gameObject.SetActive(false);
        SongSelectMenu.gameObject.SetActive(false);
        Music.PlayMenuMusic();
    }

    public void StartTrack(Track track) 
    {
        currentTrack = track;

        SetTrackDetails(track);

        StartGameplay();

        if (track.Tutorial) 
        {
            EnableGameplayUI(false);
            TutorialManager.gameObject.SetActive(true);
        }
        else 
        {
            EnableGameplayUI(true);
        }
    }

    public void StartGameplay() 
    {
        Music.PlaySong();
        Metronome.ToggleGameplay(true);
    }

    public void EnableGameplayUI(bool healthbars) 
    {
        UIManager.ToggleGameplayUI(healthbars);
        UIManager.ToggleMainMenu(false);
        SongSelectMenu.gameObject.SetActive(false);
        IndicatorManager.ToggleIndicators(true);
    }

    public void StartTutorial() 
    {
        Track tutorialTrack = Instantiate(Resources.Load<Track>("Tracks/Tutorial"));

        SetTrackDetails(tutorialTrack);

        StartGameplay();
        EnableGameplayUI(false);
    }

    public void SetTrackDetails(Track track) 
    {
        Music.audioSource.clip = track.Song;
        Metronome.ResetValues(track, GeneralOffset);
        Choreographer.ResetValues(track);
        LightingManager.SetStartBattleBeat(track.GameplayStartBeat);
        DialogueManager.AddAllTimedDialogue(track.Dialogue);
        PlayerStatus.Health.SetMaxHealth(track.PlayerHealth);
        Enemy.Health.SetMaxHealth(track.EnemyHealth);
    }

    public void BeginOffsetCalculation() 
    {
        OffsetCalculator.gameObject.SetActive(true);
        OffsetCalculator.Reset();
        GeneralOffset = 0;

        Track track = OffsetCalculator.CalculationTrack;

        SetTrackDetails(track);
        UIManager.ToggleOffsetCalculationPanel(true);
        UIManager.ToggleDisplayOffsetCalculation(false);
        StartGameplay();
    }

    public void OnCompleteOffsetCalculation() 
    {
        GeneralOffset = OffsetCalculator.GetOffsetAverageInt();
        OffsetCalculator.gameObject.SetActive(false);
        UIManager.ToggleDisplayOffsetCalculation(true);
        Metronome.PushOffset(GeneralOffset);
    }

    public void ExitOffsetCalculation() 
    {
        SaveOffset();
        UIManager.ToggleOffsetCalculationPanel(false);
        Music.audioSource?.Stop();
        Metronome.ToggleGameplay(false);
        ReturnToMenu();
    }

    public void LoseGame() 
    {
        EndBattle?.Invoke();

        ToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);
        Choreographer.ResetValues();

        UIManager.ShowLossPanel();
    }

    public void WinGame() 
    {
        EndBattle?.Invoke();

        ToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);
        Choreographer.ResetValues();

        UIManager.ShowWinPanel();
    }

    public void PauseGameplay() 
    {
        ToggleMusic?.Invoke(false);
        Metronome.ToggleGameplay(false);
    }

    public void ResumeGameplay() 
    {
        ToggleMusic?.Invoke(true);
        Metronome.ToggleGameplay(true);
    }

    public void EscapeMidSong() 
    {
        if (Metronome.GameplayActive) 
        {
            LoseGame();
        }
    }

    public void RestartGame() 
    {
        ResetGameState?.Invoke();

        StartTrack(currentTrack);
    }

    public void ReturnToMenu() 
    {
        ResetGameState?.Invoke();
        IndicatorManager.ToggleIndicators(false);
        Music.PlayMenuMusic();
    }

    public void SongSelectButton() 
    {
        ToggleSongSelect(true);
    }

    public void ToggleSongSelect(bool toggle) 
    {
        SongSelectMenu.gameObject.SetActive(toggle);
        UIManager.ToggleMainMenu(!toggle);
    }

    public void QuitGame() 
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void SaveOffset()
    {
        PlayerPrefs.SetInt("GeneralOffset", GeneralOffset);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        GeneralOffset = PlayerPrefs.GetInt("GeneralOffset", 0);
    }
}
