using System;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    public MusicController Music;

    public static event Action OnBeatWindowOpened;
    public static event Action OnBeatPassed;
    public static event Action OnBeatWindowClosed;
    public static event Action<float> OnBeatDurationSet;

    public float BPM;
    public int BeatsInBar;
    public float TimingWindowMargin = 80;
    public float Offset;
    public bool HalfBeatsEnabled = false;

    private int lastBeat;
    [SerializeField] private float beatDurationMs;
    private float nextBeatPosition;
    private int activeBeat = -1;
    private float activeBeatStartPos;
    private float activeBeatEndPos;
    private bool beatPassed = false;

    private bool gameplayActive = false;

    public void Start()
    {
        Music = GameManager.Instance.Music;

        ResetValues();
    }

    public void ResetValues()
    {
        lastBeat = 0;
        beatDurationMs = (60 / BPM) * 1000;
        OnBeatDurationSet?.Invoke(beatDurationMs / 1000);
        if (HalfBeatsEnabled) beatDurationMs /= 2;
        nextBeatPosition = beatDurationMs + Offset;
        SetNextBeatWindow();
    }

    public void ResetValues(Track track)
    {
        BPM = track.BPM;
        BeatsInBar = track.BeatsInBar;
        Offset = track.Offset;
        HalfBeatsEnabled = track.HalfBeatsEnabled;

        lastBeat = 0;
        beatDurationMs = (60 / BPM) * 1000;
        OnBeatDurationSet?.Invoke(beatDurationMs / 1000);
        if (HalfBeatsEnabled) beatDurationMs /= 2;
        nextBeatPosition = beatDurationMs + Offset;
        SetNextBeatWindow();
    }

    public float GetCurrentBeat() 
    {
        float currentSongTime = Music.SongTimeInMilliseconds();

        return currentSongTime / beatDurationMs;
    }

    public void Update()
    {
        if (!gameplayActive) return;

        float currentPosition = Music.SongTimeInMilliseconds();

        if (activeBeat == -1) 
        {
            // Open the beat timing window
            if (currentPosition >= activeBeatStartPos)
            {
                activeBeat = lastBeat + 1;
                OnBeatWindowOpened?.Invoke();
                beatPassed = false;
            }
        }

        if (currentPosition >= nextBeatPosition - 5 && !beatPassed) 
        {
            OnBeatPassed?.Invoke();
            beatPassed = true;
        }

        // Close the beat timing window
        if (currentPosition >= activeBeatEndPos) 
        {
            lastBeat += 1;
            nextBeatPosition += beatDurationMs;
            activeBeat = -1;
            SetNextBeatWindow();
            OnBeatWindowClosed?.Invoke();
        }
    }

    private void SetNextBeatWindow() 
    {
        activeBeatStartPos = nextBeatPosition - TimingWindowMargin;
        activeBeatEndPos = nextBeatPosition + TimingWindowMargin;
    }

    public int GetActiveBeat() 
    {
        return activeBeat;
    }

    public float GetTimingOfSpecificBeatInMs(int beat) 
    {
        return (beatDurationMs * beat)+ Offset;
    }

    public int GetProximityToNearestBeat() 
    {
        float currentPosition = Music.SongTimeInMilliseconds();

        int proximity = (int)Mathf.Round(currentPosition - nextBeatPosition);

        return proximity;
    }

    public int GetNextBarStartBeat() 
    {
        int currentBeat = lastBeat + 1;

        int beatsTilNextBar = currentBeat % BeatsInBar;

        return currentBeat + beatsTilNextBar;
    }

    public float GetBeatDurationMs() 
    {
        return beatDurationMs;
    }

    public void ToggleGameplay(bool toggle) 
    {
        gameplayActive = toggle;
    }
}
