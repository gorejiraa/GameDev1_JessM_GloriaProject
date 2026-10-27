using UnityEngine;
using UnityEngine.UI;

public class UIBackgroundScroller : MonoBehaviour
{
    [SerializeField] private float speedX = 0.1f;
    [SerializeField] private float speedY = 0.1f;

    private RawImage rawImage;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        rawImage.uvRect = new Rect(
            rawImage.uvRect.position + new Vector2(speedX, speedY) * Time.deltaTime,
            rawImage.uvRect.size
        );
    }
}