using UnityEngine;

public class Judge : MonoBehaviour
{
    public int NextCorrectInput = 0;
    private int LastReceivedInput = 0;
    private bool InputReceived = false;

    public void OnEnable()
    {
        Choreographer.OnBeatListen += SetNextInput;
    }

    public void SetNextInput(int input) 
    {
        InputReceived = false;
        LastReceivedInput = 0;
        NextCorrectInput = input;
    }

    public void ReceiveInput(int input)
    {
        if (!InputReceived) 
        {
            LastReceivedInput = input;
            InputReceived = true;
        }
    }

    public bool ValidateLastInput() 
    {
        return LastReceivedInput == NextCorrectInput;
    }
}
