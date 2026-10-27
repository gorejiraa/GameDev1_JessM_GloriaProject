using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEncounter : MonoBehaviour
{
    [Header("Animation Details")]
    public Sprite DefaultSprite;
    public List<NoteActionDetails> NoteActions;
    [SerializeField] private float spriteAnimationTime = 0.4f;
    [SerializeField] private bool matchAnimationTimerToTimingWindow = false;
    [SerializeField] private float spriteAnimationSlideDistance;

    [Header("Song Details")]
    public Track EncounterTrack;
    public float TimingWindow = 80;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector2 baseScale;
    private Vector2 basePosition;
    private int beatToCue = -1;
    private float spriteTimer = 0f;
    private bool spriteTimerActive = false;
    private Vector2 slideDirection;

    public Health Health;

    public static event Action<Track, float> SendEncounterDetails;
    public static event Action<Health> OnEnemyHealthChange;
    public static event Action OnEnemyHealthEmpty;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = new Health(10);

        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        baseScale = transform.localScale;
        basePosition = transform.localPosition;
        Choreographer.OnBeatDemo += DemoBeat;
        Metronome.OnBeatPassed += DemoBeatPassed;
        Judge.OnSequenceEnd += LoseHealthOnSuccessfulPlayerSequence;
        GameManager.ResetGameState += ResetState;

        if (matchAnimationTimerToTimingWindow) spriteAnimationTime = (TimingWindow * 2) / 1000;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;

        spriteTimer -= delta;

        if (spriteTimer > 0f)
        {
            float distancePercentage = spriteTimer / spriteAnimationTime;
            transform.localPosition = basePosition + slideDirection * (Mathf.Sin(distancePercentage * Mathf.PI) * spriteAnimationSlideDistance);
        }
        else if (spriteTimerActive && spriteTimer < -spriteAnimationTime) ResetSprite();
    }

    private void DemoBeat(int beat) 
    {
        beatToCue = beat;

        foreach (NoteActionDetails noteAction in NoteActions) 
        {
            if (beatToCue != (int)noteAction.Note) continue;

            SetSlideDirection(noteAction.Note);
            if (noteAction.Sprite != null) SetSpriteWithTimer(noteAction.Sprite, spriteAnimationTime);
        }
    }

    private void DemoBeatPassed() 
    {
        foreach (NoteActionDetails noteAction in NoteActions)
        {
            if (beatToCue != (int)noteAction.Note) continue;

            if (noteAction.Sound != null) audioSource.PlayOneShot(noteAction.Sound);
        }
    }

    private void SetSpriteWithTimer(Sprite sprite, float time) 
    {
        spriteRenderer.sprite = sprite;
        spriteTimer = time;
        spriteTimerActive = true;
    }

    private void SetSlideDirection(Note note) 
    {
        switch (note) 
        {
            case Note.Up: slideDirection = Vector2.up; break;
            case Note.Down: slideDirection = Vector2.down; break;
            case Note.Left: slideDirection = Vector2.left; break;
            case Note.Right: slideDirection = Vector2.right; break;
            case Note.Finish: slideDirection = Vector2.zero; break;
        }
    }
    private void ResetSprite() 
    {
        spriteRenderer.sprite = DefaultSprite;
        spriteTimerActive = false;
        transform.localPosition = basePosition;
    }

    public void BeginEncounter() 
    {
        SendEncounterDetails?.Invoke(EncounterTrack, TimingWindow);
    }

    private void TakeDamage(int damage) 
    {
        Health.TakeDamage(damage);
        OnEnemyHealthChange?.Invoke(Health);

        if (Health.Empty) OnEnemyHealthEmpty?.Invoke();
    }

    private void LoseHealthOnSuccessfulPlayerSequence(bool successfulSequence) 
    {
        if (!successfulSequence) return;

        TakeDamage(1);
    }

    private void ResetState() 
    {
        Health.Reset();
        OnEnemyHealthChange?.Invoke(Health);
        spriteTimer = 0f;
    }
}
