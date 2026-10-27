using System.Text.RegularExpressions;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private static readonly Regex GoalRegex = new Regex(@"\((?<number>\d+)\)");

    private int progressGoal;
    private int progress;

    private void Start()
    {
        GameManager.EndBattle += OnDisable;
    }

    public void ReceiveTutorialPrompt(string prompt) 
    {
        progressGoal = int.Parse(GoalRegex.Match(prompt).Groups["number"].Value);

        string cleanPrompt = GoalRegex.Replace(prompt, "").Trim();

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
        progress++;

        if (progress >= progressGoal) 
        {
            ClearSubscribers();
            progress = 0;
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
        progress = 0;
        gameObject.SetActive(false);
    }
}
