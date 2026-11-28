using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static event Action<Health> OnPlayerHealthChange;
    public static event Action OnPlayerHealthEmpty;

    public Health Health;

    [SerializeField] private AudioClip _takeDamageSound;
    private AudioSource _audioSource;

    [SerializeField] private float _damageFlickerTime = 0.3f;
    private float _damageTimer = 0f;
    private SpriteRenderer _spriteRenderer;
    private bool _invincibility = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = new Health(5);
        Judge.OnValidateInput += TakeDamageOnFailedInput;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        Choreographer.OnSwitchToListenState += ResetInvincibility;
        GameManager.ResetGameState += ResetState;
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageTimer > 0f)
        {
            _damageTimer -= Time.deltaTime;
            if (_damageTimer <= 0f) 
            {
                ToggleDamageVisual(false);
            }
        }
    }

    private void TakeDamageOnFailedInput(bool success) 
    {
        if (success) return;
        if (_invincibility) return;

        _invincibility = true;

        _audioSource.PlayOneShot(_takeDamageSound);
        Health.TakeDamage(1);
        OnPlayerHealthChange?.Invoke(Health);

        if (Health.Empty) 
        {
            OnPlayerHealthEmpty?.Invoke();
            return;
        }

        _damageTimer = _damageFlickerTime;
        ToggleDamageVisual(true);
    }

    private void ToggleDamageVisual(bool toggle) 
    {
        if (toggle) _spriteRenderer.color = Color.red;
        else _spriteRenderer.color = Color.white;
    }

    private void ResetInvincibility() 
    {
        _invincibility = false;
    }

    private void ResetState() 
    {
        Health.Reset();
        OnPlayerHealthChange?.Invoke(Health);
        _invincibility = false;
    }
}
