using UnityEngine;
using UnityEngine.UI;

public class HealthUIScript : MonoBehaviour
{
    public Slider healthBar;        // Drag your Health Bar (Slider) here in the Inspector
    public int maxHealth = 100;     // Max health
    public int currentHealth;       // Current health

    void Start()
    {
        // Initialize health to max at the start
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damageAmount)
    {
        // Decrease health
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // Prevent negative health
        UpdateHealthBar();
    }

    public void Heal(int healAmount)
    {
        // Increase health
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // Prevent exceeding max health
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Update the health bar value based on current health
        healthBar.value = (float)currentHealth / maxHealth;
    }
}