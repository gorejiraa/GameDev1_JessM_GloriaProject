using System;
using UnityEngine;

public class Judge : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private int NextCorrectInput = 0;
    [SerializeField] private int LastReceivedInput = 0;
    private double lastInputProximity;
    private bool InputReceived = false;
    public bool RanValidation = false;
    private bool SequenceEnded = false;
    private bool SuccessfulSequence = true;

    [SerializeField] private AudioClip hitSuccessSound;
    [SerializeField] private AudioClip hitFailureSound;

    public static event Action OnInputReceived;
    public static event Action<bool> OnValidateInput;
    public static event Action<bool, double> OnValidateInputWithProximity;
    public static event Action<bool> OnSequenceEnd;

    [SerializeField] private bool DebugMessages = false;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();

        Choreographer.OnBeatListen += SetNextInput;
        PlayerController.OnPlayerAction += ReceiveInput;
        Metronome.OnWindowBufferPassed += ValidateLastInput;
        Choreographer.OnEndListenPhase += EndSequenceCheck;
        GameManager.ResetGameState += ResetState;
    }

    public void SetNextInput(int input) 
    {
        if (input != 0 && DebugMessages) Debug.Log($"Judge waiting for input: {(Note)input}");

        RanValidation = false;
        InputReceived = false;
        LastReceivedInput = 0;
        NextCorrectInput = input;
    }

    public void ReceiveInput(int input, double inputTime)
    {
        if (!InputReceived) 
        {
            if (DebugMessages) Debug.Log($"Judge received player input: {(Note)input}");

            double songInputTime = GameManager.Instance.Music.RealtimeToSongMs(inputTime);
            double inputProximity = songInputTime - (GameManager.Instance.Metronome.ActiveBeatTimeMs() + GameManager.Instance.GeneralOffset);

            LastReceivedInput = input;
            lastInputProximity = inputProximity;
            InputReceived = true;
            OnInputReceived?.Invoke();

            ValidateLastInput();
        }
    }

    private void ValidateLastInput() 
    {
        if (RanValidation || NextCorrectInput == 0 || NextCorrectInput == 7) return;

        RanValidation = true;

        bool valid = false;

        if (LastReceivedInput == NextCorrectInput && Math.Abs(lastInputProximity) <= GameManager.Instance.Metronome.TimingWindowMargin) valid = true;
        else SuccessfulSequence = false;

        OnValidateInput?.Invoke(valid);
        OnValidateInputWithProximity?.Invoke(valid, lastInputProximity);

        if (valid) audioSource.PlayOneShot(hitSuccessSound);
        else audioSource.PlayOneShot(hitFailureSound);

        if (NextCorrectInput != 0 && DebugMessages) Debug.Log($"Player succeeded note: {valid} (Expected {NextCorrectInput}, Received {LastReceivedInput})");
    }

    private void EndSequenceCheck() 
    {
        OnSequenceEnd?.Invoke(SuccessfulSequence);
        SuccessfulSequence = true;
    }

    private void ResetState() 
    {
        SuccessfulSequence = true;
        LastReceivedInput = 0;
        NextCorrectInput = 0;
        InputReceived = false;
        RanValidation = false;
    }
}
