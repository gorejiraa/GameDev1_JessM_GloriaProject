using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffsetCalculator : MonoBehaviour
{
    public static event Action Complete;

    public Track CalculationTrack;

    private List<int> proximityReads = new List<int>();
    [SerializeField] private int inputsNeeded = 10;

    private void OnEnable()
    {
        Debug.Log("offset calculator enabled");
    }

    public void OnBeatPress(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        int proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat();

        proximityReads.Add(proximity);

        //Debug.Log($"Proximity: {proximity} ({proximityReads.Count}/{inputsNeeded})");

        if (proximityReads.Count >= inputsNeeded) 
        {
            Complete?.Invoke();
        }
    }

    public string GetOffsetAverageString()
    {
        int averageInputOffset = GetOffsetAverageInt();

        string read;

        if (averageInputOffset > 0) read = $"+{averageInputOffset}";
        else read = $"{averageInputOffset}";

        return read;
    }

    public int GetOffsetAverageInt() 
    {
        int average = 0;

        if (proximityReads.Count > 0) average = (int)Math.Round(proximityReads.Average());

        return average;
    }

    public void Reset()
    {
        proximityReads.Clear();
    }

    public void SetGeneralOffset() 
    {
        GameManager.Instance.GeneralOffset = GetOffsetAverageInt();
    }
}
