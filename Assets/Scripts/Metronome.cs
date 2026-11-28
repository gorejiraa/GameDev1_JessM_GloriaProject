using System;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    public static event Action OnBeatWindowOpened;
    public static event Action OnBeatPassed;
    public static event Action OnBeatWindowClosed;
    public static event Action<int> OnBeatWindowOpenedWithBeat;
    public static event Action<double> OnBeatDurationSet;

    private MusicController _music;

    [Header("Current Track Info")]
    public double BPM;
    public int BeatsInBar;
    public double TimingWindowMargin = 80;
    public double Offset;
    public bool HalfBeatsEnabled = false;

    private int _lastBeat;
    private double _nextBeatPosition;
    private double _activeBeatStartPos;
    private double _activeBeatEndPos;
    private double _firstBeatMs;
    private bool _beatPassed = false;

    [Header("Debug Info")]
    [SerializeField] private double _beatDurationMs;
    [SerializeField] private double _bufferTimeMs = 50;
    [SerializeField] private int _activeBeat = -1;

    public bool GameplayActive { get; private set; } = false;

    public void Start()
    {
        _music = GameManager.Instance.Music;

        ResetValues();
    }

    public void ResetValues()
    {
        _lastBeat = 0;
        _beatDurationMs = (60 / BPM) * 1000;
        if (HalfBeatsEnabled) _beatDurationMs /= 2;
        OnBeatDurationSet?.Invoke(_beatDurationMs / 1000);
        _nextBeatPosition = _beatDurationMs + Offset;
        SetNextBeatWindow();
    }

    public void ResetValues(Track track, int generalOffset, int timingWindow)
    {
        BPM = track.BPM;
        BeatsInBar = track.BeatsInBar;
        Offset = generalOffset;
        
        HalfBeatsEnabled = track.HalfBeatsEnabled;
        TimingWindowMargin = timingWindow;

        _activeBeat = -1;
        _lastBeat = 0;
        _beatDurationMs = (60 / BPM) * 1000;
        if (HalfBeatsEnabled) _beatDurationMs /= 2;
        OnBeatDurationSet?.Invoke(_beatDurationMs / 1000);
        _nextBeatPosition = _beatDurationMs + Offset + track.Offset;
        _firstBeatMs = _nextBeatPosition;

        if (_beatDurationMs / 2 <= TimingWindowMargin) TimingWindowMargin = _beatDurationMs * 0.5f;

        SetNextBeatWindow();
    }

    public double GetCurrentBeat() 
    {
        double currentSongTime = _music.SongTimeInMilliseconds();

        return currentSongTime / _beatDurationMs;
    }

    public void Update()
    {
        if (!GameplayActive) return;

        double currentPosition = _music.SongTimeInMilliseconds();

        if (_activeBeat == -1) 
        {
            // Open the beat timing window
            if (currentPosition >= _activeBeatStartPos)
            {
                _activeBeat = _lastBeat + 1;
                OnBeatWindowOpened?.Invoke();
                OnBeatWindowOpenedWithBeat?.Invoke(_activeBeat);
                _beatPassed = false;
            }
        }

        if (currentPosition >= _nextBeatPosition && !_beatPassed) 
        {
            OnBeatPassed?.Invoke();
            _beatPassed = true;
        }

        // Close the beat timing window
        if (currentPosition >= _activeBeatEndPos) 
        {
            _lastBeat += 1;
            _nextBeatPosition += _beatDurationMs;
            _activeBeat = -1;
            SetNextBeatWindow();
            OnBeatWindowClosed?.Invoke();
        }
    }

    private void SetNextBeatWindow() 
    {
        _activeBeatStartPos = _nextBeatPosition - TimingWindowMargin;
        _activeBeatEndPos = _nextBeatPosition + TimingWindowMargin;
    }

    public int GetActiveBeat() 
    {
        return _activeBeat;
    }

    public double GetTimingOfSpecificBeatInMs(int beat) 
    {
        return (_beatDurationMs * beat) + Offset;
    }

    public int GetProximityToNearestBeat() 
    {
        double songTimeMs = _music.SongTimeInMilliseconds();

        int beat = (int)Math.Round((songTimeMs - _firstBeatMs) / _beatDurationMs) + 1;
        return (int)Math.Round((songTimeMs - BeatTimeMs(beat)));
    }

    public void ToggleGameplay(bool toggle) 
    {
        GameplayActive = toggle;
    }

    private double BeatTimeMs(int beat) 
    {
        return _firstBeatMs + ((beat-1) * _beatDurationMs);
    }

    public double ActiveBeatTimeMs() 
    {
        return BeatTimeMs(_activeBeat);
    }

    public double NextBeatPositionDspTime() 
    {
        return GameManager.Instance.Music.SongTimeToDspTime(_nextBeatPosition / 1000);
    }
}
