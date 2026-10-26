using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Animation Details")]
    public Sprite DefaultSprite;
    public List<NoteActionDetails> NoteActions;
    [SerializeField] private float spriteAnimationTime = 0.4f;
    [SerializeField] private bool matchAnimationTimerToTimingWindow = false;
    [SerializeField] private float spriteAnimationSlideDistance;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector2 baseScale;
    private Vector2 basePosition;

    private float spriteTimer = 0f;
    private bool spriteTimerActive = false;
    private Vector2 slideDirection;

    public static event Action<int> OnPlayerAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        baseScale = transform.localScale;
        basePosition = transform.localPosition;

        EnemyEncounter.SendEncounterDetails += GetSongDetails;
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
        else if (spriteTimerActive && spriteTimer < 0) ResetSprite();
    }

    public void OnMove(Note direction)
    {
        foreach (NoteActionDetails noteAction in NoteActions)
        {
            if (direction != noteAction.Note) continue;

            SetSlideDirection(noteAction.Note);
            if (noteAction.Sprite != null) SetSpriteWithTimer(noteAction.Sprite, spriteAnimationTime);
            if (noteAction.Sound != null) audioSource.PlayOneShot(noteAction.Sound);
        }

        OnPlayerAction?.Invoke((int)direction);
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

    private void GetSongDetails(Track track, float timingWindow) 
    {
        if (matchAnimationTimerToTimingWindow) spriteAnimationTime = (timingWindow * 2) / 1000;
    }

    public void OnLeft(InputAction.CallbackContext context) 
    {
        if (!context.started) return;

        OnMove(Note.Left);
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Right);
    }

    public void OnUp(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Up);
    }

    public void OnDown(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Down);
    }
}
