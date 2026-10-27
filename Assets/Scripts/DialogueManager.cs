using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SpeakerSprite
{
    public string Name;
    public Sprite Sprite;
}

[Serializable]
public struct TimedDialogue 
{
    public float Time;
    public List<string> Dialogue;
}

[Serializable]
public struct VoicedDialogue
{
    public string Text;
    public AudioClip Audio;
}

public class DialogueManager : MonoBehaviour
{
    private static readonly Regex MetaRegex = new Regex(@"\[([a-zA-Z0-9]+)(?:=""(.*?)"")?\]");
    private static event Action<string> PromptTutorial;

    private Queue<string> DialogueQueue = new Queue<string>();
    private List<TimedDialogue> TimedDialogueSections = new List<TimedDialogue>();

    [Header("Object References")]
    [SerializeField] private GameObject DialogueCanvas;
    [SerializeField] private Image DialogueBox;
    [SerializeField] private Image SpeakerImage;
    [SerializeField] private TextMeshProUGUI DialogueText;

    [Header("Text Settings")]
    [SerializeField] private float DefaultCharPerSecond = 15;
    [SerializeField] private float DefaultAutoWaitSeconds = 5;
    [SerializeField] private float DialogueBoxOpenSpeed = 5;

    [SerializeField] private List<SpeakerSprite> SpeakerSprites;

    private string _currentText = "";
    private float _textIndex = 0;
    private bool _showingText = false;
    private bool _autoContinue = false;
    private float _autoWaitTime = 99;
    private float _charPerSecond = 5;
    private float _dialogueStartTimer = 0;
    private Vector2 _dialogueBoxSize;

    private void Start()
    {
        DialogueText.text = "";
        GameManager.EndBattle += CancelDialogue;
        OffsetCalculator.Complete += CancelDialogue;
    }

    private void Update()
    {
        CheckIfTimedDialogueShouldStart();

        if (!_showingText)
        {
            _dialogueStartTimer = Mathf.Max(_dialogueStartTimer - Time.deltaTime * DialogueBoxOpenSpeed, 0);
        }
        else
        {
            _dialogueStartTimer = Mathf.Min(_dialogueStartTimer + Time.deltaTime * DialogueBoxOpenSpeed, 1);
        }

        if (_showingText && !DialogueCanvas.activeInHierarchy) DialogueCanvas.SetActive(true);
        else if (!_showingText && DialogueCanvas.activeInHierarchy && _dialogueStartTimer <= 0) DialogueCanvas.SetActive(false);

        DialogueBox.transform.localScale = new Vector3(_dialogueStartTimer, _dialogueStartTimer, 1f);

        if (_dialogueStartTimer < 1) return;

        _textIndex = Math.Min(_currentText.Length, _textIndex + _charPerSecond * Time.deltaTime);

        string textToDisplay = _currentText.Substring(0, (int)Math.Floor(_textIndex));

        DisplayText(textToDisplay);

        if (_autoContinue && _textIndex >= _currentText.Length) 
        {
            _autoWaitTime -= Time.deltaTime;

            if (_autoWaitTime <= 0)
            {
                ProgressDialogue();
            }
        }
    }

    public void DialogueBoxClick() 
    {
        if (_textIndex >= _currentText.Length)
        {
            ProgressDialogue();
        }
        else 
        {
            _textIndex = _currentText.Length;
        }
    }

    public void InitiateDialogue(List<string> dialogue) 
    {
        foreach (string line in dialogue) 
        {
            DialogueQueue.Enqueue(line);
        }

        ProgressDialogue();
    }

    public void ProgressDialogue() 
    {
        if (DialogueQueue.TryDequeue(out string firstLine))
        {
            SetText(firstLine);
        }
        else 
        {
            _showingText = false;
        }
    }

    private void CancelDialogue() 
    {
        DialogueQueue?.Clear();
        _showingText = false;
        _dialogueStartTimer = 0;
    }

    private void SetText(string text) 
    {
        _textIndex = 0;
        _showingText = true;

        _currentText = ParseMetadata(text);
    }

    private void DisplayText(string text) 
    {
        DialogueText.text = text;
    }

    private void SetSpeakerSprite(string speakerName) 
    {
        Sprite spriteToSet = null;

        foreach (SpeakerSprite speaker in SpeakerSprites) 
        {
            if (speaker.Name == speakerName) spriteToSet = speaker.Sprite;
        }

        if (spriteToSet != null)
        {
            SpeakerImage.gameObject.SetActive(true);
            SpeakerImage.sprite = spriteToSet;
        }
        else SpeakerImage.gameObject.SetActive(false);
    }

    private string ParseMetadata(string text) 
    {
        MatchCollection matches = MetaRegex.Matches(text);

        string speakerName = null;
        bool auto = false;
        float speed = DefaultCharPerSecond;

        foreach (Match match in matches) 
        {
            string key = match.Groups[1].Value;
            string value = null;
            if (match.Groups[2].Success) value = match.Groups[2].Value;

            if (key == "speaker" || key == "s")
            {
                speakerName = value;
            }
            else if (key == "auto")
            {
                auto = true;

                if (int.TryParse(value, out int result)) _autoWaitTime = result;
                else _autoWaitTime = DefaultAutoWaitSeconds;
            }
            else if (key == "speed" || key == "spd")
            {
                if (int.TryParse(value, out int result)) speed = result;
            }
            else if (key == "tutorial")
            {
                GameManager.Instance.TutorialManager.ReceiveTutorialPrompt(value);
            }
            else if (key == "patterns") 
            {
                GameManager.Instance.Choreographer.SetActivePatterns(value);
            }
        }

        SetSpeakerSprite(speakerName);
        _autoContinue = auto;
        _charPerSecond = speed;

        string cleanText = MetaRegex.Replace(text, "").Trim();

        return cleanText;
    }

    public void AddAllTimedDialogue(List<TimedDialogue> timedDialogueList) 
    {
        if (timedDialogueList == null) return;

        foreach (TimedDialogue timedDialogue in timedDialogueList)
        {
            TimedDialogueSections.Add(timedDialogue);
        }
    }

    private void CheckIfTimedDialogueShouldStart() 
    {
        float songTime = GameManager.Instance.Music.SongTimeInSeconds();

        if (TimedDialogueSections.Count > 0) 
        {
            int index = TimedDialogueSections.FindIndex(t => songTime >= t.Time);

            if (index == -1) return;

            InitiateDialogue(TimedDialogueSections[index].Dialogue);

            TimedDialogueSections.RemoveAt(index);
        }
    }
}


