using System;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AttackProjectile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator Animator;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _spawnMoveSpeed;
    [SerializeField] private int _targetBeat;
    private bool _spawning = true;
    private bool _moving = false;
    private float stoppingDistance = 0.05f;
    private int _orderInQueue = 0;
    [SerializeField] private float _queueGap;

    private Vector3 _idleLocation;
    private Vector3 _targetLocation;

    public static event Action OnMove;

    public void Initialize(AnimatorOverrideController anim, int beat, int order, int sortingOrder, Vector3 idleLocation, Vector3 targetLocation)
    {
        Animator.runtimeAnimatorController = anim;
        _targetBeat = beat;
        _orderInQueue = order;
        _idleLocation = idleLocation;
        _targetLocation = targetLocation;
        _spriteRenderer.sortingOrder = sortingOrder - order;
    }

    private void Start()
    {
        Metronome.OnBeatWindowOpenedWithBeat += MoveOnBeat;
        OnMove += DecrementOrder;
        GameManager.EndBattle += Cancel;
        PlayerStatus.OnPlayerHealthEmpty += Cancel;
        EnemyEncounter.OnEnemyHealthEmpty += Cancel;
    }

    // Update is called once per frame
    void Update()
    {
        float colorFade = 1 - ((float)_orderInQueue / 10f);

        _spriteRenderer.color = new Color(colorFade, colorFade, colorFade, 1);

        if (!_moving)
        {
            Vector3 realIdleLocation = new Vector3(_idleLocation.x, _idleLocation.y + _orderInQueue * _queueGap, _idleLocation.z);

            float step = _spawnMoveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, realIdleLocation, step);

            if (Vector3.Distance(transform.position, realIdleLocation) <= stoppingDistance)
            {
                // Snap to the exact position and stop moving
                transform.position = realIdleLocation;
            }

        }
        else if (_moving) 
        {
            float step = _moveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, _targetLocation, step);

            if (Vector3.Distance(transform.position, _targetLocation) <= stoppingDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void MoveOnBeat(int activeBeat) 
    {
        if (activeBeat < _targetBeat) return;

        Debug.Log($"Moving on beat: {activeBeat} = {_targetBeat}");

        _moving = true;
        Animator.SetBool("Moving", true);

        Metronome.OnBeatWindowOpenedWithBeat -= MoveOnBeat;
        AttackProjectile.OnMove -= DecrementOrder;

        OnMove?.Invoke();
    }

    private void DecrementOrder() 
    {
        _orderInQueue--;
    }

    private void Cancel() 
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        // Guaranteed cleanup whenever this projectile is destroyed for ANY reason
        Metronome.OnBeatWindowOpenedWithBeat -= MoveOnBeat;
        OnMove -= DecrementOrder;
        GameManager.EndBattle -= Cancel;
        PlayerStatus.OnPlayerHealthEmpty -= Cancel;
        EnemyEncounter.OnEnemyHealthEmpty -= Cancel;
    }
}
