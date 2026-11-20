using UnityEngine;

public class IndicatorCircleColorChange : MonoBehaviour
{
    [SerializeField] private float _highlightTimeLength = 0.5f;
    private SpriteRenderer _renderer;
    private bool _highlighted;
    private float _highlightTimer = 0;
    private Color _highlightColor;
    private Color _baseColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _baseColor = _renderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (_highlighted) 
        {
            _highlightTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_highlightTimer / _highlightTimeLength);

            _renderer.color = Color.Lerp(_highlightColor, _baseColor, progress);

            if (progress == 1) 
            {
                _renderer.color = _baseColor;
                _highlighted = false;
            }
        }
    }

    public void Highlight(float timerLength, Color color) 
    {
        _highlightTimeLength = timerLength;
        _highlighted = true;
        _highlightColor = color;
        _renderer.color = color;
        _highlightTimer = 0;
    }
}
