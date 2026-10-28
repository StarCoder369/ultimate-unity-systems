using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Recovery")]
    public bool canRecover = true;
    public float recoveryAmount = 5f;
    public float recoveryDelay = 3f;
    public float recoveryInterval = 0.01f;

    [Header("UI")]
    public TMP_Text healthText;

    private float recoveryTimer;
    private float recoveryIntervalTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void Update()
    {
        if (canRecover && currentHealth < maxHealth)
        {
            recoveryTimer -= Time.deltaTime;

            if (recoveryTimer <= 0f)
            {
                recoveryIntervalTimer -= Time.deltaTime;

                if (recoveryIntervalTimer <= 0f)
                {
                    RecoverHealth(recoveryAmount);
                    recoveryIntervalTimer = recoveryInterval;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        recoveryTimer = recoveryDelay;
        recoveryIntervalTimer = 0f;

        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void RecoverHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateUI();
    }

    public void SetHealth(float amount)
    {
        currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
    }

    public void EnableRegen()
    {
        canRecover = true;
    }

    public void DisableRegen()
    {
        canRecover = false;
    }

    public void SetRegenEnabled(bool enabledOrNot)
    {
        canRecover = enabledOrNot;
    }

    public void EnableDisableRegen()
    {
        canRecover = !canRecover;
    }

    public void SetRegenAmount(float amount)
    {
        recoveryAmount = amount;
    }

    public void SetRegenInterval(float amount)
    {
        recoveryInterval = amount;
    }

    public void SetRegenDelay(float amount)
    {
        recoveryDelay = amount;
    }
}