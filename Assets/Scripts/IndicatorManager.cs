using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField] private float _highlightTimeLength = 0.5f;
    [SerializeField] private Color _highlightColor = Color.cyan;

    public GameObject LeftIndicator;
    public GameObject RightIndicator;
    public GameObject UpIndicator;
    public GameObject DownIndicator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ToggleIndicators(false);
        PlayerController.OnPlayerAction += HighlightIndicator;
    }

    public void ToggleIndicators(bool toggle) 
    {
        LeftIndicator.SetActive(toggle);
        RightIndicator.SetActive(toggle);
        UpIndicator.SetActive(toggle);
        DownIndicator.SetActive(toggle);
    }

    public Vector2 GetIndicatorPosition(Note note) 
    {
        switch (note) 
        {
            case Note.Left:
                return LeftIndicator.transform.position;
            case Note.Right:
                return RightIndicator.transform.position;
            case Note.Up:
                return UpIndicator.transform.position;
            case Note.Down:
                return DownIndicator.transform.position;
            default:
                return transform.position;
        }
    }

    public Vector3 GetIndicatorScale(Note note) 
    {
        switch (note) 
        {
            case Note.Left:
                return LeftIndicator.transform.localScale;
            case Note.Right:
                return RightIndicator.transform.localScale;
            case Note.Up:
                return UpIndicator.transform.localScale;
            case Note.Down:
                return DownIndicator.transform.localScale;
            default:
                return transform.localScale;
        }
    }

    public void HighlightIndicator(int direction, double time) 
    {
        Note note = (Note)direction;

        switch (note)
        {
            case Note.Left:
                LeftIndicator.GetComponent<IndicatorCircleColorChange>().Highlight(_highlightTimeLength, _highlightColor);
                break;
            case Note.Right:
                RightIndicator.GetComponent<IndicatorCircleColorChange>().Highlight(_highlightTimeLength, _highlightColor);
                break;
            case Note.Up:
                UpIndicator.GetComponent<IndicatorCircleColorChange>().Highlight(_highlightTimeLength, _highlightColor);
                break;
            case Note.Down:
                DownIndicator.GetComponent<IndicatorCircleColorChange>().Highlight(_highlightTimeLength, _highlightColor);
                break;
            default:
                return;
        }
    }
}
