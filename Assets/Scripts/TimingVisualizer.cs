using System;
using UnityEngine;
using UnityEngine.UI;

public class TimingVisualizer : MonoBehaviour
{
    private Image image;

    private int currentActiveBeat = -99;
    private Vector3 baseScale;

    void Start()
    {
        image = GetComponent<Image>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        int activeBeat = GameManager.Instance.Metronome.GetActiveBeat();

        if (activeBeat != currentActiveBeat)
        {
            if (activeBeat == -1)
            {
                image.color = Color.red;
            }
            else
            {
                image.color = Color.green;
            }

            currentActiveBeat = activeBeat;
        }

        float proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat();

        transform.localScale = baseScale * Math.Max((1.2f - (Math.Abs(proximity) / GameManager.Instance.Metronome.TimingWindowMargin) / 5), 1);
    }
}
