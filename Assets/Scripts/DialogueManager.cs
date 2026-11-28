using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
    public List<VoicedDialogue> Dialogue;
}

[Serializable]
public struct VoicedDialogue
{
    [TextArea]
    public string Text;
    public AudioClip Audio;
}

public class DialogueManager : MonoBehaviour
{
    private static readonly Regex _metaRegex = new Regex(@"\[([a-zA-Z0-9]+)(?:=""(.*?)"")?\]");

    private AudioSource _audioSource;

    private Queue<VoicedDialogue> _dialogueQueue = new Queue<VoicedDialogue>();
    private List<TimedDialogue> _timedDialogueSections = new List<TimedDialogue>();

    [Header("Object References")]
    [SerializeField] private GameObject _dialogueCanvas;
    [SerializeField] private Image _dialogueBox;
    [SerializeField] private Image _speakerImage;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Text Settings")]
    [SerializeField] private float _defaultCharPerSecond = 15;
    [SerializeField] private float _defaultAutoWaitSeconds = 3;
    [SerializeField] private float _dialogueBoxOpenSpeed = 5;

    [SerializeField] private List<SpeakerSprite> _speakerSprites;

    private string _currentText = "";
    private float _textIndex = 0;
    private bool _showingText = false;
    private bool _autoContinue = false;
    private float _autoWaitTime = 99;
    private float _charPerSecond = 30;
    private float _dialogueStartTimer = 0;
    private bool _progressAfterAudio = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        _dialogueText.text = "";
        GameManager.EndBattle += CancelDialogue;
        PlayerStatus.OnPlayerHealthEmpty += CancelDialogue;
        EnemyEncounter.OnEnemyHealthEmpty += CancelDialogue;
    }

    private void Update()
    {
        CheckIfTimedDialogueShouldStart();

        if (!_showingText)
        {
            _dialogueStartTimer = Mathf.Max(_dialogueStartTimer - Time.deltaTime * _dialogueBoxOpenSpeed, 0);
        }
        else
        {
            _dialogueStartTimer = Mathf.Min(_dialogueStartTimer + Time.deltaTime * _dialogueBoxOpenSpeed, 1);
        }

        if (_showingText && !_dialogueCanvas.activeInHierarchy) _dialogueCanvas.SetActive(true);
        else if (!_showingText && _dialogueCanvas.activeInHierarchy && _dialogueStartTimer <= 0) _dialogueCanvas.SetActive(false);

        _dialogueBox.transform.localScale = new Vector3(_dialogueStartTimer, _dialogueStartTimer, 1f);

        if (_dialogueStartTimer < 1) return;

        _textIndex = Math.Min(_currentText.Length, _textIndex + _charPerSecond * Time.deltaTime);

        string textToDisplay = _currentText.Substring(0, (int)Math.Floor(_textIndex));

        DisplayText(textToDisplay);

        if (_autoContinue) 
        {
            if (_progressAfterAudio)
            {
                if (_audioSource.time >= _audioSource.clip.length) ProgressDialogue();
            }
            else if (_textIndex >= _currentText.Length)
            {
                _autoWaitTime -= Time.deltaTime;

                if (_autoWaitTime <= 0)
                {
                    ProgressDialogue();
                }
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

    public void InitiateDialogue(List<VoicedDialogue> dialogue) 
    {
        foreach (VoicedDialogue line in dialogue) 
        {
            _dialogueQueue.Enqueue(line);
        }

        ProgressDialogue();
    }

    public void ProgressDialogue() 
    {
        _progressAfterAudio = false;

        if (_dialogueQueue.TryDequeue(out VoicedDialogue line))
        {
            SetText(line.Text);
            if (line.Audio != null)
            {
                _audioSource.clip = line.Audio;
                _audioSource.Play();
                _progressAfterAudio = true;
            }
        }
        else 
        {
            _showingText = false;
        }
    }

    private void CancelDialogue() 
    {
        _timedDialogueSections?.Clear();
        _dialogueQueue?.Clear();
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
        _dialogueText.text = text;
    }

    private void SetSpeakerSprite(string speakerName) 
    {
        Sprite spriteToSet = null;

        foreach (SpeakerSprite speaker in _speakerSprites) 
        {
            if (speaker.Name == speakerName) spriteToSet = speaker.Sprite;
        }

        if (spriteToSet != null)
        {
            _speakerImage.gameObject.SetActive(true);
            _speakerImage.sprite = spriteToSet;
        }
        else _speakerImage.gameObject.SetActive(false);
    }

    private string ParseMetadata(string text) 
    {
        MatchCollection matches = _metaRegex.Matches(text);

        string speakerName = null;
        bool auto = true;
        float speed = _defaultCharPerSecond;
        _autoWaitTime = _defaultAutoWaitSeconds;

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
                if (int.TryParse(value, out int result)) _autoWaitTime = result;
                else if (value == "false") auto = false;
                else _autoWaitTime = _defaultAutoWaitSeconds;
            }
            else if (key == "speed" || key == "spd")
            {
                if (int.TryParse(value, out int result)) speed = result;
            }
            else if (key == "tutorial")
            {
                auto = false;
                GameManager.Instance.TutorialManager.ReceiveTutorialPrompt(value);
            }
            else if (key == "patterns") 
            {
                GameManager.Instance.Choreographer.SetActivePatterns(value);
            }
        }

        SetSpeakerSprite(speakerName);
        _charPerSecond = speed;
        _autoContinue = auto;

        string cleanText = _metaRegex.Replace(text, "").Trim();

        return cleanText;
    }

    public void AddAllTimedDialogue(List<TimedDialogue> timedDialogueList) 
    {
        if (timedDialogueList == null) return;

        foreach (TimedDialogue timedDialogue in timedDialogueList)
        {
            _timedDialogueSections.Add(timedDialogue);
        }
    }

    private void CheckIfTimedDialogueShouldStart() 
    {
        double songTime = GameManager.Instance.Music.SongTimeInSeconds();

        if (_timedDialogueSections.Count > 0) 
        {
            int index = _timedDialogueSections.FindIndex(t => songTime >= t.Time);

            if (index == -1) return;

            InitiateDialogue(_timedDialogueSections[index].Dialogue);

            _timedDialogueSections.RemoveAt(index);
        }
    }
}


