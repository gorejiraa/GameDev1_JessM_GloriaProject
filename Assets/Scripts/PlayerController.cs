using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action<int, double> OnPlayerAction;

    [Header("Animation Details")]
    public Sprite DefaultSprite;
    public List<NoteActionDetails> NoteActions;
    [SerializeField] private float _spriteAnimationTime = 0.4f;
    [SerializeField] private bool _matchAnimationTimerToTimingWindow = false;
    [SerializeField] private float _spriteAnimationSlideDistance;

    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private Vector2 _basePosition;

    private float _spriteTimer = 0f;
    private bool _spriteTimerActive = false;
    private Vector2 _slideDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _basePosition = transform.localPosition;

        EnemyEncounter.SendEncounterDetails += GetSongDetails;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;

        _spriteTimer -= delta;

        if (_spriteTimer > 0f)
        {
            float distancePercentage = _spriteTimer / _spriteAnimationTime;
            transform.localPosition = _basePosition + _slideDirection * (Mathf.Sin(distancePercentage * Mathf.PI) * _spriteAnimationSlideDistance);
        }
        else if (_spriteTimerActive && _spriteTimer < 0) ResetSprite();
    }

    public void OnMove(Note direction, double time)
    {
        foreach (NoteActionDetails noteAction in NoteActions)
        {
            if (direction != noteAction.Note) continue;

            SetSlideDirection(noteAction.Note);
            if (noteAction.Sprite != null) SetSpriteWithTimer(noteAction.Sprite, _spriteAnimationTime);
            if (noteAction.Sound != null) _audioSource.PlayOneShot(noteAction.Sound);
        }

        OnPlayerAction?.Invoke((int)direction, time);
    }

    private void SetSpriteWithTimer(Sprite sprite, float time)
    {
        _spriteRenderer.sprite = sprite;
        _spriteTimer = time;
        _spriteTimerActive = true;
    }

    private void SetSlideDirection(Note note)
    {
        switch (note)
        {
            case Note.Up: _slideDirection = Vector2.up; break;
            case Note.Down: _slideDirection = Vector2.down; break;
            case Note.Left: _slideDirection = Vector2.left; break;
            case Note.Right: _slideDirection = Vector2.right; break;
            case Note.Finish: _slideDirection = Vector2.zero; break;
        }
    }
    private void ResetSprite()
    {
        _spriteRenderer.sprite = DefaultSprite;
        _spriteTimerActive = false;
        transform.localPosition = _basePosition;
    }

    private void GetSongDetails(Track track, float timingWindow) 
    {
        if (_matchAnimationTimerToTimingWindow) _spriteAnimationTime = (timingWindow * 2) / 1000;
    }

    public void OnLeft(InputAction.CallbackContext context) 
    {
        if (!context.started) return;

        OnMove(Note.Left, context.time);
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Right, context.time);
    }

    public void OnUp(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Up, context.time);
    }

    public void OnDown(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        OnMove(Note.Down, context.time);
    }
}
