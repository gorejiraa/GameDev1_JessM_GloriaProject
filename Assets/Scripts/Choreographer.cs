using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Choreographer : MonoBehaviour
{
    private static readonly Regex PatternSetRegex = new Regex(@"\d+");

    [SerializeField] private int NextDemoBeat;
    [SerializeField] private int NextListenBeat;
    [SerializeField] private int ActiveBeat;
    public ChoreographerState State = ChoreographerState.Waiting;
    public int BeatsBeforeSetting = 4;

    private List<Note> _currentPattern;
    private int _currentPatternBeat = 0;
    private bool _randomPatterns = true;
    private int _currentPatternLength = 8;
    private bool _currentPatternHalfBeats = false;
    private List<Pattern> _allPresetPatterns;
    private List<Pattern> _activePresetPatterns;

    [SerializeField] private bool DisableBlanks = false;
    [SerializeField] private bool LastSequenceNoteIsGo = false;
    [SerializeField] private bool DebugMessages = false;

    public static event Action<int> OnBeatDemo;
    public static event Action<int> OnBeatListen;
    public static event Action OnSwitchToDemoState;
    public static event Action OnSwitchToListenState;
    public static event Action OnEndListenPhase;

    [SerializeField] private GameObject notePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetValues();
    }

    public void ResetValues() 
    {
        State = ChoreographerState.Waiting;
        _currentPattern = new List<Note>();
        _currentPatternLength = GameManager.Instance.Metronome.BeatsInBar;
        ActiveBeat = 0;
        NextDemoBeat = GameManager.Instance.Metronome.BeatsInBar + 1;
        NextListenBeat = NextDemoBeat;
        _currentPatternBeat = 0;
    }

    public void ResetValues(Track track)
    {
        State = ChoreographerState.Waiting;
        _currentPattern = new List<Note>();
        ActiveBeat = 0;
        NextDemoBeat = track.GameplayStartBeat + 1;
        NextListenBeat = NextDemoBeat;
        _currentPatternBeat = 0;
        _randomPatterns = track.RandomPatterns;
        _allPresetPatterns = track.Patterns.ToList();

        if (track.RandomPatterns) _currentPatternLength = track.BeatsInBar;

        if (_allPresetPatterns.Count <= 0) _randomPatterns = true;
        if (!track.HalfBeatsEnabled) _allPresetPatterns.RemoveAll(p => p.HalfBeats);

        _activePresetPatterns = _allPresetPatterns.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.Metronome.GetActiveBeat() != -1) ActiveBeat = GameManager.Instance.Metronome.GetActiveBeat();

        if (State == ChoreographerState.Waiting) 
        {
            if (ActiveBeat >= NextDemoBeat - BeatsBeforeSetting) State = ChoreographerState.Setting;
        }
        else if (State == ChoreographerState.Setting)
        {
            SetNextPattern();
            NextListenBeat += _currentPatternLength;
            State = ChoreographerState.Demonstrating;
            OnSwitchToDemoState?.Invoke();
        }
        else if (State == ChoreographerState.Demonstrating)
        {
            if (ActiveBeat == NextDemoBeat)
            {
                OnBeatListen.Invoke(0);

                // Demo the beat using pattern[CurrentPatternBeat]
                OnBeatDemo.Invoke((int)_currentPattern[_currentPatternBeat]);
                float noteHitTime = GameManager.Instance.Metronome.GetTimingOfSpecificBeatInMs(ActiveBeat+_currentPattern.Count);
                SpawnNote(noteHitTime, (Note)_currentPattern[_currentPatternBeat]);

                if (DebugMessages) Debug.Log($"Demo: {_currentPattern[_currentPatternBeat]} (Bar {_currentPatternBeat + 1}/{_currentPatternLength}");

                if (!_currentPatternHalfBeats && GameManager.Instance.Metronome.HalfBeatsEnabled) NextDemoBeat += 2;
                else NextDemoBeat++;

                _currentPatternBeat++;
            }
            if (NextDemoBeat == NextListenBeat)
            {
                NextDemoBeat += _currentPatternLength;
                _currentPatternBeat = 0;
                State = ChoreographerState.Listening;
                OnSwitchToListenState?.Invoke();
            }
        }
        else if (State == ChoreographerState.Listening)
        {
            if (ActiveBeat == NextListenBeat)
            {
                OnBeatDemo.Invoke(-1);

                // Tell judge to listen for pattern[CurrentBeatPattern]
                OnBeatListen.Invoke((int)_currentPattern[_currentPatternBeat]);
                if (DebugMessages) Debug.Log($"Listening for: {_currentPattern[_currentPatternBeat]} (Bar {_currentPatternBeat + 1}/{_currentPatternLength}");

                if (!_currentPatternHalfBeats && GameManager.Instance.Metronome.HalfBeatsEnabled) NextListenBeat += 2;
                else NextListenBeat++;

                _currentPatternBeat++;
            }
            if (NextListenBeat == NextDemoBeat)
            {
                _currentPatternBeat = 0;
                State = ChoreographerState.Setting;
                OnEndListenPhase?.Invoke();
            }
        }
    }

    public void SpawnNote(float hitTime, Note note)
    {
        if (note == Note.Blank || note == Note.Finish) return;

        // Pass the prefab reference, position, and rotation
        GameObject newNote = Instantiate(notePrefab);
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
        if (DisableBlanks) minRange = 1;

        _currentPattern.Clear();

        for (int i = 0; i < _currentPatternLength; i++)
        {
            int note = UnityEngine.Random.Range(minRange, 5);
            _currentPattern.Add((Note)note);
        }

        if (LastSequenceNoteIsGo) _currentPattern[_currentPattern.Count - 1] = Note.Finish;

        if (DebugMessages) Debug.Log($"Next random pattern: {string.Join(", ", _currentPattern)}");
    }

    private void SelectRandomPatternPreset() 
    {
        int index = UnityEngine.Random.Range(0, _activePresetPatterns.Count);

        _currentPattern = _activePresetPatterns[index].Notes;
        _currentPatternLength = _currentPattern.Count;
        _currentPatternHalfBeats = _activePresetPatterns[index].HalfBeats;

        if (DebugMessages) Debug.Log($"Selected preset pattern {index}: {string.Join(", ", _currentPattern)}");
    }

    public void SetActivePatterns(string patternIndexes) 
    {
        if (DebugMessages) Debug.Log("Setting active patterns...");

        _activePresetPatterns.Clear();

        MatchCollection matches = PatternSetRegex.Matches(patternIndexes);

        foreach (Match match in matches)
        {
            int index = int.Parse(match.Value);

            if (DebugMessages) Debug.Log($"Activating pattern: {index}");

            _activePresetPatterns.Add(_allPresetPatterns[index]);
        }
    }
}

public enum ChoreographerState 
{
    Setting,
    Waiting,
    Demonstrating,
    Listening
}
