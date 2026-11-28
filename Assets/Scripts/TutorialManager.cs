using System.Text.RegularExpressions;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private static readonly Regex _goalRegex = new Regex(@"\((?<number>\d+)\)");

    private int _progressGoal;
    private int _progress;

    private void Start()
    {
        GameManager.EndBattle += OnDisable;
    }

    public void ReceiveTutorialPrompt(string prompt) 
    {
        _progressGoal = int.Parse(_goalRegex.Match(prompt).Groups["number"].Value);

        string cleanPrompt = _goalRegex.Replace(prompt, "").Trim();

        switch (cleanPrompt) 
        {
            case "Hit":
                Judge.OnValidateInput += IncrementProgressConditional;
                break;
            case "Sequence":
                Judge.OnSequenceEnd += IncrementProgressConditional;
                break;
            case "End":
                GameManager.Instance.WinGame();
                break;
            default:
                break;
        }
    }

    private void IncrementProgressConditional(bool valid)
    {
        if (!valid) return;

        IncrementProgress();
    }

    private void IncrementProgress() 
    {
        _progress++;

        if (_progress >= _progressGoal) 
        {
            ClearSubscribers();
            _progress = 0;
            GameManager.Instance.DialogueManager.ProgressDialogue();
        }
    }

    private void ClearSubscribers() 
    {
        Judge.OnValidateInput -= IncrementProgressConditional;
        Judge.OnSequenceEnd -= IncrementProgressConditional;
    }

    private void OnDisable()
    {
        ClearSubscribers();
        _progress = 0;
        gameObject.SetActive(false);
    }
}
