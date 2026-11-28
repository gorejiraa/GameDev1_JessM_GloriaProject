using System;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TimingVisualizer : MonoBehaviour
{
    private Image image;

    private int _currentActiveBeat = -99;
    private Vector3 _baseScale;
    private int _offset = 0;

    void Start()
    {
        image = GetComponent<Image>();
        _baseScale = transform.localScale;
        OffsetCalculator.Complete += GetOffset;
    }

    void Update()
    {
        float proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat() + _offset;

        float t = Mathf.InverseLerp((float)GameManager.Instance.Metronome.TimingWindowMargin, 0f, Mathf.Abs(proximity));
        float scaleMultiplier = Mathf.Lerp(1f, 1.4f, t);

        transform.localScale = _baseScale * scaleMultiplier;

        if (t == 0)
        {
            image.color = Color.red;
        }
        else
        {
            image.color = Color.green;
        }
    }

    private void GetOffset(int offset) 
    {
        _offset = offset;
    }
}
