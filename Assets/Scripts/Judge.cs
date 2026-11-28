using System;
using System.Drawing;
using UnityEngine;

public enum Judgement
{
    Miss,
    OK,
    Great,
    Perfect
}

public class Judge : MonoBehaviour
{
    public static event Action OnInputReceived;
    public static event Action<bool> OnValidateInput;
    public static event Action<Judgement, double> OnJudgementRating;
    public static event Action<bool> OnSequenceEnd;

    [Header("Judgements")]
    [SerializeField] private int _perfectRange;
    [SerializeField] private int _greatRange;

    private double _lastInputProximity;
    private bool _inputReceived = false;
    private bool _ranValidation = false;
    private bool _successfulSequence = true;

    [Header("Audio")]
    [SerializeField] private AudioClip _hitSuccessSound;
    [SerializeField] private AudioClip _hitFailureSound;
    private AudioSource _audioSource;

    [Header("Debugging")]
    [SerializeField] private int _nextCorrectInput = 0;
    [SerializeField] private int _lastReceivedInput = 0;
    [SerializeField] private bool _debugMessages = false;

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        Choreographer.OnBeatListen += SetNextInput;
        PlayerController.OnPlayerAction += ReceiveInput;
        Metronome.OnBeatWindowClosed += ValidateLastInput;
        Choreographer.OnEndListenPhase += EndSequenceCheck;
        GameManager.ResetGameState += ResetState;
    }

    public void SetNextInput(int input) 
    {
        if (input != 0 && _debugMessages) Debug.Log($"Judge waiting for input: {(Note)input}");

        _ranValidation = false;
        _inputReceived = false;
        _lastReceivedInput = 0;
        _nextCorrectInput = input;
    }

    public void ReceiveInput(int input, double inputTime)
    {
        if (!_inputReceived) 
        {
            if (_debugMessages) Debug.Log($"Judge received player input: {(Note)input}");

            double songInputTime = GameManager.Instance.Music.RealtimeToSongMs(inputTime);
            double inputProximity = songInputTime - (GameManager.Instance.Metronome.ActiveBeatTimeMs() + GameManager.Instance.GeneralOffset);

            _lastReceivedInput = input;
            _lastInputProximity = inputProximity;
            _inputReceived = true;
            OnInputReceived?.Invoke();

            ValidateLastInput();
        }
    }

    private void ValidateLastInput() 
    {
        if (_ranValidation || _nextCorrectInput == 0 || _nextCorrectInput == 7) return;

        _ranValidation = true;

        bool valid = false;

        if (_lastReceivedInput == _nextCorrectInput && Math.Abs(_lastInputProximity) <= GameManager.Instance.Metronome.TimingWindowMargin) valid = true;
        else _successfulSequence = false;

        Judgement judgement = valid ? GetJudgementFromProximity(_lastInputProximity) : Judgement.Miss;

        OnValidateInput?.Invoke(valid);
        OnJudgementRating?.Invoke(judgement, _lastInputProximity);

        if (valid) _audioSource.PlayOneShot(_hitSuccessSound);
        else _audioSource.PlayOneShot(_hitFailureSound);

        if (_nextCorrectInput != 0 && _debugMessages) Debug.Log($"Player succeeded note: {valid} (Expected {_nextCorrectInput}, Received {_lastReceivedInput})");
    }

    private void EndSequenceCheck() 
    {
        OnSequenceEnd?.Invoke(_successfulSequence);
        _successfulSequence = true;
    }

    private Judgement GetJudgementFromProximity(double proximity) 
    {
        if (Math.Abs(proximity) < _perfectRange)
        {
            return Judgement.Perfect;
        }
        else if (Math.Abs(proximity) < _greatRange)
        {
            return Judgement.Great;
        }
        else
        {
            return Judgement.OK;
        }
    }

    private void ResetState() 
    {
        _successfulSequence = true;
        _lastReceivedInput = 0;
        _nextCorrectInput = 0;
        _inputReceived = false;
        _ranValidation = false;
    }
}
