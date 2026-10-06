using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VisualDemo : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    public AudioClip tap;
    public AudioClip dont;
    public AudioClip go;
    public TextMeshProUGUI currentBeatText;
    public TextMeshProUGUI offsetText;

    private int currentActiveBeat = -99;
    private List<int> proximityReads = new List<int>();
    [SerializeField] private int averageInputOffset = 0;
    private int proximity = 0;
    private Vector3 baseScale;
    private int beatToCue = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        baseScale = transform.localScale;
        Choreographer.OnBeatDemo += CueBeat;
        Metronome.OnBeatPassed += DemoBeat;
    }

    // Update is called once per frame
    void Update()
    {
        int activeBeat = GameManager.Instance.Metronome.GetActiveBeat();

        if (activeBeat != currentActiveBeat)
        {
            if (activeBeat == -1)
            {
                spriteRenderer.color = Color.red;
            }
            else
            {
                spriteRenderer.color = Color.green;
            }

            if (currentBeatText != null) currentBeatText.text = activeBeat.ToString();

            currentActiveBeat = activeBeat;
        }

        proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat();

        transform.localScale = baseScale * Math.Max((1.2f - (Math.Abs(proximity) / GameManager.Instance.Metronome.TimingWindowMargin) / 5), 1);
    }

    public void StartButton() 
    {
        GameManager.Instance.Music.PlaySong();
        GameManager.Instance.Metronome.ResetValues();

        proximityReads.Clear();
    }

    public void OnBeatPress(InputAction.CallbackContext context) 
    {
        if (!context.started) return;

        GameManager.Instance.Judge.ReceiveInput(1);

        proximityReads.Add(proximity);

        string read;

        if (proximity > 0) read = $"+{proximity}";
        else read = $"{proximity}";

        Debug.Log($"Offset: {read}");

        averageInputOffset = (int)Math.Round(proximityReads.Average());

        if (offsetText != null) offsetText.text = averageInputOffset.ToString();
    }

    public void DemoBeat() 
    {
        if (beatToCue == 1) audioSource.PlayOneShot(tap); 
        //else if (beatToCue == 0) audioSource.PlayOneShot(dont);
        else if (beatToCue == 7) audioSource.PlayOneShot(go);
    }

    public void CueBeat(int beat) 
    {
        beatToCue = beat;
    }
}
