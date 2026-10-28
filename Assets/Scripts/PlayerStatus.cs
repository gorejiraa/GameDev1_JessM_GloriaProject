using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public Health Health;

    [SerializeField] private float DamageFlickerTime = 0.3f;
    private float DamageTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool Invincibility = false;

    public static event Action<Health> OnPlayerHealthChange;
    public static event Action OnPlayerHealthEmpty;

    private AudioSource audioSource;
    [SerializeField] private AudioClip TakeDamageSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = new Health(5);
        Judge.OnValidateInput += TakeDamageOnFailedInput;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        Choreographer.OnSwitchToListenState += ResetInvincibility;
        GameManager.ResetGameState += ResetState;
    }

    // Update is called once per frame
    void Update()
    {
        if (DamageTimer > 0f)
        {
            DamageTimer -= Time.deltaTime;
            if (DamageTimer <= 0f) 
            {
                ToggleDamageVisual(false);
            }
        }
    }

    private void TakeDamageOnFailedInput(bool success) 
    {
        if (success) return;
        if (Invincibility) return;

        Invincibility = true;

        audioSource.PlayOneShot(TakeDamageSound);
        Health.TakeDamage(1);
        OnPlayerHealthChange?.Invoke(Health);

        if (Health.Empty) 
        {
            OnPlayerHealthEmpty?.Invoke();
            return;
        }

        DamageTimer = DamageFlickerTime;
        ToggleDamageVisual(true);
    }

    private void ToggleDamageVisual(bool toggle) 
    {
        if (toggle) spriteRenderer.color = Color.red;
        else spriteRenderer.color = Color.white;
    }

    private void ResetInvincibility() 
    {
        Invincibility = false;
    }

    private void ResetState() 
    {
        Health.Reset();
        OnPlayerHealthChange?.Invoke(Health);
        Invincibility = false;
    }
}
