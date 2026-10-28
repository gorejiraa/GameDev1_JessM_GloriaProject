using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float TargetHitTime { get; private set; }
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Timing Variables")]
    [SerializeField] private float PreBeatsVisualTime = 500;
    [SerializeField] private float MsTilDestruction = 300;

    [Header("Scale Boundaries")]
    [SerializeField] private float startingScaleRatio = 4.0f;
    [SerializeField] private float startingScale = 4.0f;
    [SerializeField] private float targetScale = 1.0f;

    [Header("Fade Boundaries")]
    [SerializeField] private float maxOpacity = 0.5f;
    [SerializeField] private float fadeDurationProgress = 0.2f;

    public void Initialize(float hitTime, Note note)
    {
        TargetHitTime = hitTime;
        transform.position = GameManager.Instance.IndicatorManager.GetIndicatorPosition(note);
        targetScale = GameManager.Instance.IndicatorManager.GetIndicatorScale(note).x;
        startingScale = targetScale * startingScaleRatio;
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
    }

    public float GetProgress()
    {
        float timeRemaining = TargetHitTime - GameManager.Instance.Music.SongTimeInMilliseconds();

        if (timeRemaining <= -MsTilDestruction) Destroy(gameObject);

        float progress = 1f - (timeRemaining / PreBeatsVisualTime);

        return Mathf.Clamp01(progress);
    }

    void Update()
    {
        float currentProgress = GetProgress();

        Color currentColor = spriteRenderer.color;

        if (currentProgress <= fadeDurationProgress)
        {
            float fadeProgress = currentProgress / fadeDurationProgress;
            currentColor.a = Mathf.Lerp(0f, maxOpacity, fadeProgress);
        }
        else
        {
            currentColor.a = maxOpacity;
        }

        spriteRenderer.color = currentColor;

        float currentScale = Mathf.Lerp(startingScale, targetScale, currentProgress);
        transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }

}
