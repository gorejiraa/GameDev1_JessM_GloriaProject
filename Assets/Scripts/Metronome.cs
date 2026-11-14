using System;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    public MusicController Music;

    public static event Action OnBeatWindowOpened;
    public static event Action OnBeatPassed;
    public static event Action OnBeatWindowClosed;
    public static event Action<double> OnBeatDurationSet;

    public double BPM;
    public int BeatsInBar;
    public double TimingWindowMargin = 80;
    public double Offset;
    public bool HalfBeatsEnabled = false;

    private int lastBeat;
    [SerializeField] private double beatDurationMs;
    public double NextBeatPosition;
    [SerializeField] private int activeBeat = -1;
    private double activeBeatStartPos;
    private double activeBeatEndPos;
    private bool beatPassed = false;
    private double firstBeatMs;

    public bool GameplayActive { get; private set; } = false;

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
        NextBeatPosition = beatDurationMs + Offset;
        SetNextBeatWindow();
    }

    public void ResetValues(Track track, int generalOffset)
    {
        BPM = track.BPM;
        BeatsInBar = track.BeatsInBar;
        Offset = generalOffset;
        firstBeatMs = track.Offset;
        HalfBeatsEnabled = track.HalfBeatsEnabled;
        TimingWindowMargin = track.TimingWindow;

        activeBeat = -1;
        lastBeat = 0;
        beatDurationMs = (60 / BPM) * 1000;
        OnBeatDurationSet?.Invoke(beatDurationMs / 1000);
        if (HalfBeatsEnabled) beatDurationMs /= 2;
        NextBeatPosition = beatDurationMs + Offset;
        SetNextBeatWindow();
    }

    public double GetCurrentBeat() 
    {
        double currentSongTime = Music.SongTimeInMilliseconds();

        return currentSongTime / beatDurationMs;
    }

    public void Update()
    {
        if (!GameplayActive) return;

        double currentPosition = Music.SongTimeInMilliseconds();

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

        if (currentPosition >= NextBeatPosition && !beatPassed) 
        {
            OnBeatPassed?.Invoke();
            beatPassed = true;
        }

        // Close the beat timing window
        if (currentPosition >= activeBeatEndPos) 
        {
            lastBeat += 1;
            NextBeatPosition += beatDurationMs;
            activeBeat = -1;
            SetNextBeatWindow();
            OnBeatWindowClosed?.Invoke();
        }
    }

    private void SetNextBeatWindow() 
    {
        activeBeatStartPos = NextBeatPosition - TimingWindowMargin;
        activeBeatEndPos = NextBeatPosition + TimingWindowMargin;
    }

    public int GetActiveBeat() 
    {
        return activeBeat;
    }

    public double GetTimingOfSpecificBeatInMs(int beat) 
    {
        return (beatDurationMs * beat) + Offset;
    }

    public int GetProximityToNearestBeat() 
    {
        double songTimeMs = Music.SongTimeInMilliseconds();

        int beat = (int)Math.Round((songTimeMs - firstBeatMs) / beatDurationMs);
        return (int)Math.Round((songTimeMs - BeatTimeMs(beat)));
    }

    public int GetNextBarStartBeat() 
    {
        int currentBeat = lastBeat + 1;

        int beatsTilNextBar = currentBeat % BeatsInBar;

        return currentBeat + beatsTilNextBar;
    }

    public double GetBeatDurationMs() 
    {
        return beatDurationMs;
    }

    public void ToggleGameplay(bool toggle) 
    {
        GameplayActive = toggle;
    }

    private double BeatTimeMs(int beat) 
    {
        return firstBeatMs + beat * beatDurationMs;
    }

    public double ActiveBeatTimeMs() 
    {
        return BeatTimeMs(activeBeat);
    }

    public double NextBeatPositionDspTime() 
    {
        return GameManager.Instance.Music.SongTimeToDspTime(NextBeatPosition / 1000);
    }
}
