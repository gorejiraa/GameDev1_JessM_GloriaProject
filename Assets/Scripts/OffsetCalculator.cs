using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffsetCalculator : MonoBehaviour
{
    public static event Action<int> Complete;

    public Track CalculationTrack;
    public GameObject OffsetDialogueBox;
    public TextMeshProUGUI OffsetDialogueText;

    [SerializeField] private int _inputsNeeded = 10;
    private List<int> _proximityReads = new List<int>();

    [SerializeField] private bool _debugMessages = false;

    private void OnEnable()
    {
        Debug.Log("offset calculator enabled");
        OffsetDialogueBox.SetActive(true);
    }

    private void OnDisable()
    {
        OffsetDialogueBox.SetActive(false);
    }

    public void OnBeatPress(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        int proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat();

        _proximityReads.Add(proximity);

        if (_proximityReads.Count >= _inputsNeeded) 
        {
            Complete?.Invoke(GetOffsetAverageInt());
            OffsetDialogueText.SetText("This is your input offset! If it doesn't seem correct, try recalibrating again!");
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

        if (_proximityReads == null || _proximityReads.Count == 0) return average;

        if (_debugMessages) Debug.Log($"Proximity reads: ({string.Join(", ", _proximityReads)})");
        if (_debugMessages) Debug.Log($"Average before trimming: ({string.Join(", ", (int)Math.Round(_proximityReads.Average()))})");

        var sorted = _proximityReads.OrderBy(x => x).ToList();

        sorted.RemoveRange(8, 2);
        sorted.RemoveRange(0, 2);

        if (_debugMessages) Debug.Log($"Proximity reads after trimming ends: ({string.Join(", ", sorted)})");

        average = (int)Math.Round(sorted.Average());

        if (_debugMessages) Debug.Log($"Average after trimming: ({average})");

        return average;
    }

    public void Reset()
    {
        _proximityReads.Clear();
    }
}
