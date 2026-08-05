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


    [Header("UI")]
    public TMP_Text healthText;


    private float recoveryTimer;


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
                RecoverHealth(recoveryAmount);
            }
        }
    }


    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);


        recoveryTimer = recoveryDelay;


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
}