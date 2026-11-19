using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AttackProjectile : MonoBehaviour
{
    [SerializeField] private Animator Animator;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _spawnMoveSpeed;
    [SerializeField] private int _targetBeat;
    private bool _spawning = true;
    private bool _moving = false;
    private float stoppingDistance = 0.05f;

    private Vector3 _idleLocation;
    private Vector3 _targetLocation;

    public void Initialize(AnimatorOverrideController anim, int beat, Vector3 idleLocation, Vector3 targetLocation)
    {
        Animator.runtimeAnimatorController = anim;
        _targetBeat = beat;
        _idleLocation = idleLocation;
        _targetLocation = targetLocation;
    }

    private void Start()
    {
        Metronome.OnBeatWindowOpenedWithBeat += MoveOnBeat;
    }

    // Update is called once per frame
    void Update()
    {
        if (_spawning)
        {
            float step = _spawnMoveSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, _idleLocation, step);

            if (Vector3.Distance(transform.position, _idleLocation) <= stoppingDistance)
            {
                // Snap to the exact position and stop moving
                transform.position = _idleLocation;
                _spawning = false;
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
    }
}
