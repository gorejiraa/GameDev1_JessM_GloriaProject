using System;
using UnityEngine;

public class Choreographer : MonoBehaviour
{
    private int[] pattern;
    private int NextDemoBeat;
    private int NextListenBeat;
    private int CurrentPatternBeat = 0;
    public ChoreographerState State = ChoreographerState.Waiting;
    public int BeatsBeforeSetting = 4;

    public static event Action<int> OnBeatDemo;
    public static event Action<int> OnBeatListen;
    public static event Action OnSwitchToDemoState;
    public static event Action OnSwitchToListenState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetValues();
    }

    public void ResetValues() 
    {
        pattern = new int[GameManager.Instance.Metronome.BeatsInBar];
        NextDemoBeat = GameManager.Instance.Metronome.BeatsInBar + 1;
        NextListenBeat = NextDemoBeat + GameManager.Instance.Metronome.BeatsInBar;
    }

    public void ResetValues(int firstBeat)
    {
        pattern = new int[GameManager.Instance.Metronome.BeatsInBar];
        NextDemoBeat = firstBeat;
        NextListenBeat = NextDemoBeat + GameManager.Instance.Metronome.BeatsInBar;
    }

    // Update is called once per frame
    void Update()
    {
        if (State == ChoreographerState.Waiting) 
        {
            if (GameManager.Instance.Metronome.GetActiveBeat() >= NextDemoBeat - BeatsBeforeSetting) State = ChoreographerState.Setting;
        }
        else if (State == ChoreographerState.Setting)
        {
            SetNextPattern();
            State = ChoreographerState.Demonstrating;
            OnSwitchToDemoState?.Invoke();
        }
        else if (State == ChoreographerState.Demonstrating)
        {
            if (GameManager.Instance.Metronome.GetActiveBeat() == NextDemoBeat)
            {
                // Demo the beat using pattern[CurrentPatternBeat]
                if (CurrentPatternBeat == 7) OnBeatDemo.Invoke(7);
                else OnBeatDemo.Invoke(pattern[CurrentPatternBeat]);
                //Debug.Log($"Demo: {pattern[CurrentPatternBeat]} (Bar {CurrentPatternBeat + 1}/{pattern.Length}");

                NextDemoBeat++;
                CurrentPatternBeat++;
            }
            if (NextDemoBeat == NextListenBeat)
            {
                NextDemoBeat += GameManager.Instance.Metronome.BeatsInBar;
                CurrentPatternBeat = 0;
                State = ChoreographerState.Listening;
                OnSwitchToListenState?.Invoke();
            }
        }
        else if (State == ChoreographerState.Listening)
        {
            if (GameManager.Instance.Metronome.GetActiveBeat() == NextListenBeat)
            {
                OnBeatDemo.Invoke(-1);
                // Tell judge to listen for pattern[CurrentBeatPattern]
                OnBeatListen.Invoke(pattern[CurrentPatternBeat]);
                //Debug.Log($"Listening for: {pattern[CurrentPatternBeat]} (Bar {CurrentPatternBeat + 1}/{pattern.Length}");

                NextListenBeat++;
                CurrentPatternBeat++;
            }
            if (NextListenBeat == NextDemoBeat)
            {
                NextListenBeat += GameManager.Instance.Metronome.BeatsInBar;
                CurrentPatternBeat = 0;
                State = ChoreographerState.Setting;
            }
        }
    }

    public void SetNextPattern()
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            int note = UnityEngine.Random.Range(1, 5);
            pattern[i] = note;
        }

        pattern[0] = 2;

        pattern[pattern.Length - 1] = 7;

        Debug.Log($"Next pattern: {string.Join(", ", pattern)}");
    }
}

public enum ChoreographerState 
{
    Setting,
    Waiting,
    Demonstrating,
    Listening
}
