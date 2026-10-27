using System;

public class Health
{
    public int MaxHealth;
    public int CurrentHealth;
    public bool Empty = false;

    public Health(int max) 
    {
        MaxHealth = max;
        CurrentHealth = max;
    }

    public void TakeDamage(int damage) 
    {
        if (damage <= 0) return;

        CurrentHealth = Math.Max(CurrentHealth - damage, 0);

        if (CurrentHealth <= 0) Empty = true;
    }

    public void Heal(int healing) 
    {
        if (healing <= 0) return;

        CurrentHealth = Math.Min(CurrentHealth + healing, MaxHealth);
    }

    public void Reset() 
    {
        CurrentHealth = MaxHealth;
        Empty = false;
    }

    public void SetMaxHealth(int health)
    {
        MaxHealth = health;
        CurrentHealth = health;
    }
}
