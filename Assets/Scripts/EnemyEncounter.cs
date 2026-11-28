using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEncounter : MonoBehaviour
{
    public static event Action<Track, float> SendEncounterDetails;
    public static event Action<Health> OnEnemyHealthChange;
    public static event Action OnEnemyHealthEmpty;

    public Health Health;
    private bool _invincible = false;

    [Header("Animation Details")]
    public Sprite DefaultSprite;
    public List<NoteActionDetails> NoteActions;
    [SerializeField] private float _spriteAnimationTime = 0.4f;
    [SerializeField] private bool _matchAnimationTimerToTimingWindow = false;
    [SerializeField] private float _spriteAnimationSlideDistance;

    [Header("Song Details")]
    public Track EncounterTrack;
    public float TimingWindow = 80;

    [Header("Attacks")]
    public AttackPositions AttackPositions;
    public AnimatorOverrideController LeftAttackController;
    public AnimatorOverrideController RightAttackController;
    public AnimatorOverrideController UpAttackController;
    public AnimatorOverrideController DownAttackController;
    public GameObject AttackPrefab;

    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private Vector2 _basePosition;
    private int _beatToCue = -1;
    private float _spriteTimer = 0f;
    private bool _spriteTimerActive = false;
    private Vector2 _slideDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = new Health(10);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _basePosition = transform.localPosition;
        Choreographer.OnBeatDemo += DemoBeat;
        Choreographer.OnNoteSpawned += SpawnAttackProjectile;
        Metronome.OnBeatPassed += DemoBeatPassed;
        Judge.OnSequenceEnd += LoseHealthOnSuccessfulPlayerSequence;
        GameManager.ResetGameState += ResetState;
        Choreographer.OnPatternZero += ToggleInvincibility;

        if (_matchAnimationTimerToTimingWindow) _spriteAnimationTime = (TimingWindow * 2) / 1000;
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
        else if (_spriteTimerActive && _spriteTimer < -_spriteAnimationTime) ResetSprite();
    }

    private void DemoBeat(int beat) 
    {
        _beatToCue = beat;

        foreach (NoteActionDetails noteAction in NoteActions) 
        {
            if (_beatToCue != (int)noteAction.Note) continue;

            SetSlideDirection(noteAction.Note);
            if (noteAction.Sprite != null) SetSpriteWithTimer(noteAction.Sprite, _spriteAnimationTime);
            _audioSource.clip = noteAction.Sound;
            if (noteAction.Sound != null) _audioSource.PlayScheduled(GameManager.Instance.Metronome.NextBeatPositionDspTime());
        }
    }

    private void DemoBeatPassed() 
    {
        foreach (NoteActionDetails noteAction in NoteActions)
        {
            if (_beatToCue != (int)noteAction.Note) continue;

            //if (noteAction.Sound != null) audioSource.PlayOneShot(noteAction.Sound);
        }
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

    public void BeginEncounter() 
    {
        SendEncounterDetails?.Invoke(EncounterTrack, TimingWindow);
    }

    private void TakeDamage(int damage) 
    {
        if (_invincible) return;

        Health.TakeDamage(damage);
        OnEnemyHealthChange?.Invoke(Health);

        if (Health.Empty) OnEnemyHealthEmpty?.Invoke();
    }

    private void LoseHealthOnSuccessfulPlayerSequence(bool successfulSequence) 
    {
        if (!successfulSequence) return;

        TakeDamage(1);
    }

    private void SpawnAttackProjectile(int targetBeat, Note note, int orderInQueue) 
    {
        AnimatorOverrideController anim = null;
        Vector3 idlePos = new Vector3(0, 0);
        int baseSortingOrder = 20;

        switch (note) 
        {
            case Note.Left:
                anim = LeftAttackController;
                idlePos = AttackPositions.Left.position;
                break;
            case Note.Right:
                anim = RightAttackController;
                idlePos = AttackPositions.Right.position;
                break;
            case Note.Up:
                anim = UpAttackController;
                idlePos = AttackPositions.Up.position;
                break;
            case Note.Down:
                anim = DownAttackController;
                idlePos = AttackPositions.Down.position;
                baseSortingOrder = 60;
                break;
            default:
                anim = null;
                break;
        }

        if (anim == null) return;

        GameObject attack = Instantiate(AttackPrefab);
        attack.GetComponent<AttackProjectile>().Initialize(anim, targetBeat, orderInQueue, baseSortingOrder, idlePos, AttackPositions.Target.position);
    }

    private void ResetState() 
    {
        Health.Reset();
        OnEnemyHealthChange?.Invoke(Health);
        _spriteTimer = 0f;
        _beatToCue = 0;
        _invincible = false;
    }

    private void ToggleInvincibility(bool toggle) 
    {
        _invincible = toggle;
    }
}
