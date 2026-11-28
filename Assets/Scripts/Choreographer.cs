using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public enum ChoreographerState
{
    Setting,
    Waiting,
    Demonstrating,
    Listening
}

public class Choreographer : MonoBehaviour
{
    public static event Action<int> OnBeatDemo;
    public static event Action<int> OnBeatListen;
    public static event Action OnSwitchToDemoState;
    public static event Action OnSwitchToListenState;
    public static event Action OnEndListenPhase;
    public static event Action<int, Note, int> OnNoteSpawned;
    public static event Action<bool> OnPatternZero;

    private static readonly Regex _patternSetRegex = new Regex(@"\d+");

    
    public int BeatsBeforeSetting = 4;
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private PresetPatternList _patternList;

    private List<Pattern> _allPresetPatterns;
    private List<Pattern> _activePresetPatterns;
    private List<Note> _currentPattern;

    private int _currentPatternBeat = 0;
    private bool _randomPatterns = true;
    private int _currentPatternLength = 8;
    private int _beatsInBar = 8;

    [Header("Debugging")]
    [SerializeField] private bool _debugMessages = false;
    [SerializeField] private ChoreographerState _state = ChoreographerState.Waiting;
    [SerializeField] private int _nextDemoBeat;
    [SerializeField] private int _nextListenBeat;
    [SerializeField] private int _activeBeat;
    [SerializeField] private bool _disableBlanks = false;
    [SerializeField] private bool _lastSequenceNoteIsGo = false;
    [SerializeField] private bool _halfBeats = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetValues();
        _allPresetPatterns = _patternList.Patterns.ToList();
    }

    public void ResetValues() 
    {
        _state = ChoreographerState.Waiting;
        _currentPattern = new List<Note>();
        _beatsInBar = GameManager.Instance.Metronome.BeatsInBar;
        _currentPatternLength = _beatsInBar;
        _activeBeat = 0;
        _nextDemoBeat = GameManager.Instance.Metronome.BeatsInBar + 1;
        _nextListenBeat = _nextDemoBeat;
        _currentPatternBeat = 0;
    }

    public void ResetValues(Track track)
    {
        _state = ChoreographerState.Waiting;
        _currentPattern = new List<Note>();
        _halfBeats = track.HalfBeatsEnabled;
        _activeBeat = 0;
        _nextDemoBeat = _halfBeats ? (track.GameplayStartBeat * 2) + 1 : track.GameplayStartBeat + 1;
        _nextListenBeat = _nextDemoBeat;
        _currentPatternBeat = 0;
        _randomPatterns = track.RandomPatterns;
        

        _beatsInBar = track.BeatsInBar;

        _currentPatternLength = _beatsInBar;

        if (_allPresetPatterns.Count <= 0) _randomPatterns = true;

        _activePresetPatterns = _allPresetPatterns.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.Metronome.GetActiveBeat() != -1) _activeBeat = GameManager.Instance.Metronome.GetActiveBeat();

        if (_state == ChoreographerState.Waiting) 
        {
            if (_activeBeat >= _nextDemoBeat - BeatsBeforeSetting) _state = ChoreographerState.Setting;
        }
        else if (_state == ChoreographerState.Setting)
        {
            SetNextPattern();
            _nextListenBeat += _currentPatternLength;
            _state = ChoreographerState.Demonstrating;
            OnSwitchToDemoState?.Invoke();
        }
        else if (_state == ChoreographerState.Demonstrating)
        {
            if (_activeBeat == _nextDemoBeat)
            {
                OnBeatListen.Invoke(0);

                // Demo the beat using pattern[CurrentPatternBeat]
                OnBeatDemo.Invoke((int)_currentPattern[_currentPatternBeat]);
                float noteHitTime = (float)GameManager.Instance.Metronome.GetTimingOfSpecificBeatInMs(_activeBeat+_currentPatternLength);
                SpawnNote(noteHitTime, (Note)_currentPattern[_currentPatternBeat]);

                if (_debugMessages) Debug.Log($"Demo: {_currentPattern[_currentPatternBeat]} (Bar {_currentPatternBeat + 1}/{_currentPatternLength})");

                _nextDemoBeat++;

                _currentPatternBeat++;
            }
            if (_nextDemoBeat == _nextListenBeat)
            {
                _nextDemoBeat += _currentPatternLength;
                _currentPatternBeat = 0;
                _state = ChoreographerState.Listening;
                OnSwitchToListenState?.Invoke();
            }
        }
        else if (_state == ChoreographerState.Listening)
        {
            if (_activeBeat == _nextListenBeat)
            {
                OnBeatDemo.Invoke(-1);

                // Tell judge to listen for pattern[CurrentBeatPattern]
                OnBeatListen.Invoke((int)_currentPattern[_currentPatternBeat]);
                if (_debugMessages) Debug.Log($"Listening for: {_currentPattern[_currentPatternBeat]} (Bar {_currentPatternBeat + 1}/{_currentPatternLength})");

                _nextListenBeat++;

                _currentPatternBeat++;
            }
            if (_nextListenBeat == _nextDemoBeat)
            {
                _currentPatternBeat = 0;
                _state = ChoreographerState.Setting;
                OnEndListenPhase?.Invoke();
            }
        }
    }

    public void SpawnNote(float hitTime, Note note)
    {
        if (note == Note.Blank || note == Note.Finish) return;

        // Pass the prefab reference, position, and rotation
        OnNoteSpawned.Invoke(_activeBeat + _currentPatternLength, note, _currentPatternBeat);
        GameObject newNote = Instantiate(_notePrefab);
        newNote.GetComponent<NoteController>().Initialize(hitTime, note);
    }

    public void SetNextPattern()
    {
        if (_randomPatterns)
        {
            GenerateRandomPattern();
        }
        else 
        {
            SelectRandomPatternPreset();
        }
    }

    private void GenerateRandomPattern() 
    {
        int minRange = 0;
        if (_disableBlanks) minRange = 1;

        _currentPattern.Clear();

        for (int i = 0; i < _currentPatternLength; i++)
        {
            int note = UnityEngine.Random.Range(minRange, 5);
            _currentPattern.Add((Note)note);
        }

        if (_lastSequenceNoteIsGo) _currentPattern[_currentPatternLength - 1] = Note.Finish;

        if (_debugMessages) Debug.Log($"Next random pattern: {string.Join(", ", _currentPattern)}");
    }

    private void SelectRandomPatternPreset() 
    {
        int index = UnityEngine.Random.Range(0, _activePresetPatterns.Count);

        SetCurrentPattern(index);

        if (_debugMessages) Debug.Log($"Selected preset pattern {index}: {string.Join(", ", _currentPattern)}");
    }

    private void SetCurrentPattern(int index) 
    {
        _currentPattern = new List<Note>(_activePresetPatterns[index].Notes);

        if (!_activePresetPatterns[index].HalfBeats && GameManager.Instance.Metronome.HalfBeatsEnabled) 
        {
            for (int i = _currentPattern.Count - 1; i >= 0; i--)
            {
                _currentPattern.Insert(i + 1, Note.Blank);
            }
        }

        _currentPatternLength = _halfBeats ? _beatsInBar * 2 : _beatsInBar;
    }

    public void SetActivePatterns(string patternIndexes) 
    {
        if (_debugMessages) Debug.Log("Setting active patterns...");

        _activePresetPatterns.Clear();

        MatchCollection matches = _patternSetRegex.Matches(patternIndexes);

        foreach (Match match in matches)
        {
            int index = int.Parse(match.Value);

            if (index == 0) OnPatternZero?.Invoke(true);
            else OnPatternZero?.Invoke(false);

            if (_debugMessages) Debug.Log($"Activating pattern: {index}");

            _activePresetPatterns.Add(_allPresetPatterns[index]);
        }
    }
}
